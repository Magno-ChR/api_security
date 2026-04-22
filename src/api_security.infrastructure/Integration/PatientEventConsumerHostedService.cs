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

namespace api_security.infrastructure.Integration;

/// <summary>Consume los eventos patient.created y patient.updated desde RabbitMQ y sincroniza en la tabla Patient.</summary>
internal sealed class PatientEventConsumerHostedService : BackgroundService
{
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

                // No declarar exchange ni cola; la cola ms-security-queue ya existe en ms-infrastructure (definitions.json)
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += (_, ea) => ProcessMessageAsync(ea);

                _channel.BasicConsume(
                    _options.QueueName,
                    autoAck: false,
                    consumer);

                _logger.LogInformation(
                    "Patient event consumer started. Host: {Host}, Queue: {Queue}",
                    _options.HostName,
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
                        "RabbitMQ: fallo al conectar/consumir (intento {Attempt}). Reintento en {Seconds}s.",
                        attempt,
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

    private async Task ProcessMessageAsync(BasicDeliverEventArgs ea)
    {
        var routingKey = ea.RoutingKey;
        var isCreated = string.Equals(routingKey, _options.PatientCreatedRoutingKey, StringComparison.Ordinal);
        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);

        _logger.LogInformation("Received message. RoutingKey: {RoutingKey}, BodyLength: {Length}", routingKey, json.Length);

        try
        {
            var dto = JsonSerializer.Deserialize<PatientIntegrationEventDto>(json, JsonOptions);
            if (dto is null)
            {
                _logger.LogWarning("Invalid message body (null after deserialize), skipping: {Body}", json);
                Ack(ea);
                return;
            }
            if (dto.PatientId == Guid.Empty)
            {
                _logger.LogWarning("Invalid message body (PatientId empty), skipping: {Body}", json);
                Ack(ea);
                return;
            }

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
            Ack(ea);
            _logger.LogInformation("Patient sync completed. PatientId: {PatientId}, IsCreated: {IsCreated}", dto.PatientId, isCreated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message. RoutingKey: {RoutingKey}, Body: {Body}", routingKey, json);
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
