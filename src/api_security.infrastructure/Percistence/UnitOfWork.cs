using api_security.domain.Abstractions;
using api_security.infrastructure.Percistence.DomainModel;
using Joseco.Outbox.Contracts.Model;
using Joseco.Outbox.Contracts.Service;
using Joseco.Outbox.EFCore.Persistence;
using System.Collections.Immutable;

namespace api_security.infrastructure.Percistence
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly DomainDbContext context;
        private readonly IOutboxService<DomainEvent> _outboxService;
        private readonly IOutboxDatabase<DomainEvent> _outboxDatabase;

        public UnitOfWork(
            DomainDbContext context,
            IOutboxService<DomainEvent> outboxService,
            IOutboxDatabase<DomainEvent> outboxDatabase)
        {
            this.context = context;
            _outboxService = outboxService;
            _outboxDatabase = outboxDatabase;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            var domainEvents = context.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.DomainEvents.Any())
                .Select(x =>
                {
                    var domainEvents = x.Entity
                                    .DomainEvents
                                    .ToImmutableArray();
                    x.Entity.ClearDomainEvents();

                    return domainEvents;
                })
                .SelectMany(domainEvents => domainEvents)
                .ToList();

            foreach (var domainEvent in domainEvents)
            {
                var message = new OutboxMessage<DomainEvent>(domainEvent);
                await _outboxService.AddAsync(message);
            }

            await context.SaveChangesAsync(cancellationToken);
            await _outboxDatabase.CommitAsync(cancellationToken);
        }
    }
}
