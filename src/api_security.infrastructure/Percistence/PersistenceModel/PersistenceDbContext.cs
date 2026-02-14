using api_security.domain.Abstractions;
using api_security.infrastructure.Percistence.PersistenceModel.Entities;
using Microsoft.EntityFrameworkCore;


namespace api_security.infrastructure.Percistence.PersistenceModel
{
    internal class PersistenceDbContext : DbContext, IDatabase
    {
        public DbSet<CredentialPM> Credentials { get; set; }
        public DbSet<UserPM> Users { get; set; }
        public DbSet<UserRolePM> UserRoles { get; set; }
        public DbSet<PatientPM> Patients { get; set; }

        public PersistenceDbContext(DbContextOptions<PersistenceDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ignorar los DomainEvent para que EF no intente mapearlos como entidades
            modelBuilder.Ignore<DomainEvent>();

            base.OnModelCreating(modelBuilder);
        }

        public void Migrate()
        {
            Database.Migrate();
        }

    }
}
