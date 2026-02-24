using api_security.domain.Abstractions;
using api_security.infrastructure.Percistence.PersistenceModel;
using Joseco.Outbox.Contracts.Model;
using Joseco.Outbox.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace api_security.infrastructure.Percistence.Outbox;

internal sealed class OutboxDatabase : IOutboxDatabase<DomainEvent>
{
    private readonly PersistenceDbContext _dbContext;

    public OutboxDatabase(PersistenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public DbSet<OutboxMessage<DomainEvent>> GetOutboxMessages() => _dbContext.OutboxMessages;

    public async Task CommitAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.SaveChangesAsync(cancellationToken);
}
