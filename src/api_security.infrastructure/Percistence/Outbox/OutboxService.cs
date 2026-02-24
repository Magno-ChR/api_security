using api_security.domain.Abstractions;
using Joseco.Outbox.Contracts.Model;
using Joseco.Outbox.Contracts.Service;
using Joseco.Outbox.EFCore.Persistence;

namespace api_security.infrastructure.Percistence.Outbox;

/// <summary>
/// Implementación propia de IOutboxService para evitar depender de AddOutbox del paquete
/// (incompatible con MediatR 14).
/// </summary>
internal sealed class OutboxService : IOutboxService<DomainEvent>
{
    private readonly IOutboxDatabase<DomainEvent> _database;

    public OutboxService(IOutboxDatabase<DomainEvent> database)
    {
        _database = database;
    }

    public Task AddAsync(OutboxMessage<DomainEvent> message)
    {
        _database.GetOutboxMessages().Add(message);
        return Task.CompletedTask;
    }
}
