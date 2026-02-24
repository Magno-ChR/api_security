using api_security.infrastructure.Percistence;
using api_security.infrastructure.Percistence.DomainModel;
using api_security.infrastructure.Percistence.Outbox;
using api_security.infrastructure.Percistence.PersistenceModel;
using api_security.infrastructure.Percistence.Repositories;
using api_security.domain.Abstractions;
using api_security.domain.Entities.Patients;
using api_security.domain.Entities.Users;
using api_security.domain.Entities.UserRoles;
using api_security.application;
using api_security.application.Common.Security;
using api_security.infrastructure.Security;
using Joseco.Outbox.Contracts.Service;
using Joseco.Outbox.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace api_security.infrastructure
{
    public static class DependencyInyection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplication()
                .AddPersistence(configuration);

            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddJwtAuthentication(configuration);

            return services;
        }

        /// <summary>
        /// Registra el servicio en segundo plano que procesa los mensajes del outbox.
        /// Usar solo en el proyecto Worker, no en la API.
        /// </summary>
        public static IServiceCollection AddOutboxBackgroundService(this IServiceCollection services, int delay = 5000)
        {
            services.AddHostedService(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<OutboxBackgroundWorkerService>>();
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                return new OutboxBackgroundWorkerService(logger, scopeFactory, delay);
            });
            return services;
        }

        private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<DomainDbContext>(context =>
                    context.UseNpgsql(connectionString));
            services.AddDbContext<PersistenceDbContext>(context =>
                    context.UseNpgsql(connectionString));

            services.AddScoped<IDatabase, PersistenceDbContext>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();

            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

            services.AddScoped<IOutboxDatabase<DomainEvent>, OutboxDatabase>();
            services.AddScoped<IOutboxService<DomainEvent>, OutboxService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
