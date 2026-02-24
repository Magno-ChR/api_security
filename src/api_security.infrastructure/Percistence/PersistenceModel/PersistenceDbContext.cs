using api_security.domain.Abstractions;
using api_security.infrastructure.Percistence.DomainModel;
using api_security.infrastructure.Percistence.PersistenceModel.Entities;
using Joseco.Outbox.Contracts.Model;
using Microsoft.EntityFrameworkCore;


namespace api_security.infrastructure.Percistence.PersistenceModel
{
    internal class PersistenceDbContext : DbContext, IDatabase
    {
        public DbSet<CredentialPM> Credentials { get; set; }
        public DbSet<UserPM> Users { get; set; }
        public DbSet<UserRolePM> UserRoles { get; set; }
        public DbSet<PatientPM> Patients { get; set; }
        public DbSet<OutboxMessage<DomainEvent>> OutboxMessages { get; set; }

        public PersistenceDbContext(DbContextOptions<PersistenceDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<DomainEvent>();
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public void Migrate()
        {
            Database.Migrate();
        }

    }
}
