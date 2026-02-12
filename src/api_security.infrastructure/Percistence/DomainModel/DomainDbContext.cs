using api_security.domain.Abstractions;
using api_security.domain.Entities.Credentials;
using api_security.domain.Entities.UserRoles;
using api_security.domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using System.Reflection;


namespace api_security.infrastructure.Percistence.DomainModel
{
    internal class DomainDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Credential> Credentials { get; set; }

        public DomainDbContext(DbContextOptions<DomainDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<DomainEvent>();
        }
    }
}
