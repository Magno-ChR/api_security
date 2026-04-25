using System.Diagnostics;
using System.Text;
using System.Text.Json;
using api_security.application.Integration.Patients;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace api_security.infrastructure.Integration;

/// <summary>Consume los eventos patient.created y patient.updated desde RabbitMQ y sincroniza en la tabla Patient.</summary>
internal sealed class PatientEventConsumerHostedService : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new(IntegrationTelemetry.ActivitySourceName);
    private readonly ILogger<PatientEventConsumerHostedService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private IConnection? _connection;
    private IModel? _channel;

    public PatientEventConsumerHostedService(
        ILogger<PatientEventConsumerHostedService> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.FromSeconds(Math.Max(0, _options.ReconnectDelaySeconds));
        var maxAttempts = _options.MaxReconnectAttempts;
        var attempt = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            attempt++;
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _options.HostName,
                    Port = _options.Port,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost ?? "/",
                    DispatchConsumersAsync = true,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = delay
                };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                EnsureQueueBindings(_channel);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += (_, ea) => ProcessMessageAsync(ea);

                _channel.BasicConsume(
                    _options.QueueName,
                    autoAck: false,
                    consumer);

                _logger.LogInformation(
                    "Patient event consumer started. Host: {Host}, Exchange: {Exchange}, Queue: {Queue}",
                    _options.HostName,
                    _options.ExchangeName,
                    _options.QueueName);
                attempt = 0;
                await Task.Delay(Timeout.Infinite, stoppingToken);
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                SafeDisposeConnection();

                var burstExhausted = maxAttempts > 0 && attempt >= maxAttempts;
                if (burstExhausted)
                {
                    _logger.LogError(
                        ex,
                        "RabbitMQ: error tras {Max} intentos consecutivos; reiniciando ciclo (el worker no se detiene).",
                        maxAttempts);
                    attempt = 0;
                }
                else
                {
                    _logger.LogWarning(
                        ex,
                        "RabbitMQ: fallo al conectar/consumir (intento {Attempt}) hacia {Host}:{Port}. Reintento en {Seconds}s.",
                        attempt,
                        _options.HostName,
                        _options.Port,
                        delay.TotalSeconds);
                }

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }

    private void SafeDisposeConnection()
    {
        try
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "RabbitMQ: error al liberar conexión/canal antes de reintentar.");
        }
        finally
        {
            _channel = null;
            _connection = null;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private void EnsureQueueBindings(IModel channel)
    {
        channel.ExchangeDeclare(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        try
        {
            channel.QueueDeclarePassive(_options.QueueName);
        }
        catch (OperationInterruptedException ex)
        {
            throw new InvalidOperationException(
                $"RabbitMQ queue '{_options.QueueName}' was not found while configuring patient event consumption.",
                ex);
        }

        channel.QueueBind(_options.QueueName, _options.ExchangeName, _options.PatientCreatedRoutingKey);
        channel.QueueBind(_options.QueueName, _options.ExchangeName, _options.PatientUpdatedRoutingKey);
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs ea)
    {
        var routingKey = ea.RoutingKey;
        var isCreated = string.Equals(routingKey, _options.PatientCreatedRoutingKey, StringComparison.Ordinal);
        var isUpdated = string.Equals(routingKey, _options.PatientUpdatedRoutingKey, StringComparison.Ordinal);
        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);
        var messageId = ea.BasicProperties?.MessageId;
        var correlationId = ea.BasicProperties?.CorrelationId;
        using var activity = ActivitySource.StartActivity("rabbitmq consume patient event", ActivityKind.Consumer);

        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination.name", _options.ExchangeName);
        activity?.SetTag("messaging.destination.kind", "exchange");
        activity?.SetTag("messaging.rabbitmq.destination.routing_key", routingKey);
        activity?.SetTag("messaging.operation.type", "process");
        activity?.SetTag("messaging.message.id", messageId);
        activity?.SetTag("messaging.conversation_id", correlationId);
        activity?.SetTag("messaging.rabbitmq.delivery_tag", ea.DeliveryTag);
        activity?.SetTag("messaging.rabbitmq.queue", _options.QueueName);

        _logger.LogInformation(
            "RabbitMQ message received. Exchange: {Exchange}, Queue: {Queue}, RoutingKey: {RoutingKey}, DeliveryTag: {DeliveryTag}, MessageId: {MessageId}, CorrelationId: {CorrelationId}, TraceId: {TraceId}, BodyLength: {Length}",
            ea.Exchange,
            _options.QueueName,
            routingKey,
            ea.DeliveryTag,
            messageId,
            correlationId,
            activity?.TraceId.ToString(),
            json.Length);

        try
        {
            if (!isCreated && !isUpdated)
            {
                _logger.LogWarning(
                    "RabbitMQ message skipped due to unsupported routing key. Exchange: {Exchange}, RoutingKey: {RoutingKey}, DeliveryTag: {DeliveryTag}, TraceId: {TraceId}, Body: {Body}",
                    ea.Exchange,
                    routingKey,
                    ea.DeliveryTag,
                    activity?.TraceId.ToString(),
                    json);
                activity?.SetStatus(ActivityStatusCode.Ok, "unsupported routing key skipped");
                Ack(ea);
                return;
            }

            var dto = JsonSerializer.Deserialize<PatientIntegrationEventDto>(json, JsonOptions);
            if (dto is null)
            {
                _logger.LogWarning(
                    "RabbitMQ message skipped because deserialization returned null. RoutingKey: {RoutingKey}, DeliveryTag: {DeliveryTag}, TraceId: {TraceId}, Body: {Body}",
                    routingKey,
                    ea.DeliveryTag,
                    activity?.TraceId.ToString(),
                    json);
                activity?.SetStatus(ActivityStatusCode.Error, "deserialization returned null");
                Ack(ea);
                return;
            }
            if (dto.PatientId == Guid.Empty)
            {
                _logger.LogWarning(
                    "RabbitMQ message skipped because PatientId is empty. RoutingKey: {RoutingKey}, DeliveryTag: {DeliveryTag}, EventId: {EventId}, TraceId: {TraceId}, Body: {Body}",
                    routingKey,
                    ea.DeliveryTag,
                    dto.Id,
                    activity?.TraceId.ToString(),
                    json);
                activity?.SetStatus(ActivityStatusCode.Error, "patient id empty");
                Ack(ea);
                return;
            }

            activity?.SetTag("patient.id", dto.PatientId);
            activity?.SetTag("patient.event.id", dto.Id);
            activity?.SetTag("patient.event.type", isCreated ? "patient.created" : "patient.updated");

            _logger.LogInformation(
                "Processing patient integration event. EventId: {EventId}, EventType: {EventType}, PatientId: {PatientId}, OccurredOn: {OccurredOn}, DocumentNumber: {DocumentNumber}, TraceId: {TraceId}",
                dto.Id,
                isCreated ? "patient.created" : "patient.updated",
                dto.PatientId,
                dto.OccurredOn,
                dto.DocumentNumber,
                activity?.TraceId.ToString());

            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var command = new SyncPatientFromIntegrationCommand
            {
                PatientId = dto.PatientId,
                IsCreated = isCreated,
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                DocumentNumber = dto.DocumentNumber
            };
            await mediator.Send(command);
            activity?.SetStatus(ActivityStatusCode.Ok);
            Ack(ea);
            _logger.LogInformation(
                "RabbitMQ message acknowledged after patient sync. EventId: {EventId}, PatientId: {PatientId}, EventType: {EventType}, DeliveryTag: {DeliveryTag}, TraceId: {TraceId}",
                dto.Id,
                dto.PatientId,
                isCreated ? "patient.created" : "patient.updated",
                ea.DeliveryTag,
                activity?.TraceId.ToString());
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("exception.type", ex.GetType().FullName);
            activity?.SetTag("exception.message", ex.Message);
            activity?.SetTag("exception.stacktrace", ex.StackTrace);
            _logger.LogError(
                ex,
                "Error processing RabbitMQ message. Exchange: {Exchange}, RoutingKey: {RoutingKey}, DeliveryTag: {DeliveryTag}, MessageId: {MessageId}, CorrelationId: {CorrelationId}, TraceId: {TraceId}, Body: {Body}",
                ea.Exchange,
                routingKey,
                ea.DeliveryTag,
                messageId,
                correlationId,
                activity?.TraceId.ToString(),
                json);
            Nack(ea, requeue: false);
        }
    }

    private void Ack(BasicDeliverEventArgs ea)
    {
        _channel?.BasicAck(ea.DeliveryTag, false);
    }

    private void Nack(BasicDeliverEventArgs ea, bool requeue)
    {
        _channel?.BasicNack(ea.DeliveryTag, false, requeue);
    }

    public override void Dispose()
    {
        SafeDisposeConnection();
        base.Dispose();
    }
}
