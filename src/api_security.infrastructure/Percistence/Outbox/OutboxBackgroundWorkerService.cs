using api_security.domain.Abstractions;
using Joseco.Outbox.Contracts.Model;
using Joseco.Outbox.EFCore.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace api_security.infrastructure.Percistence.Outbox;

/// <summary>
/// Servicio en segundo plano que procesa los mensajes pendientes del outbox
/// y los publica mediante MediatR.
/// </summary>
public sealed class OutboxBackgroundWorkerService : BackgroundService
{
    private readonly ILogger<OutboxBackgroundWorkerService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly int _delayMs;

    public OutboxBackgroundWorkerService(
        ILogger<OutboxBackgroundWorkerService> logger,
        IServiceScopeFactory scopeFactory,
        int delayMs = 5000)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _delayMs = delayMs;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<IOutboxDatabase<DomainEvent>>();
                var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

                var messages = database.GetOutboxMessages()
                    .Where(m => !m.Processed)
                    .OrderBy(m => m.Created)
                    .Take(20)
                    .ToList();

                foreach (var message in messages)
                {
                    if (message.Content is null)
                        continue;

                    try
                    {
                        await publisher.Publish(message.Content, stoppingToken);
                        message.MarkAsProcessed();
                        await database.CommitAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando mensaje outbox {MessageId}", message.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el worker de outbox: {Message}", ex.Message);
            }

            await Task.Delay(_delayMs, stoppingToken);
        }
    }
}
