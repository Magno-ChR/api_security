using api_security.infrastructure.Percistence;
using api_security.infrastructure.Percistence.DomainModel;
using api_security.infrastructure.Percistence.PersistenceModel;
using api_security.infrastructure.Percistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using patient.application;
using api_security.domain.Abstractions;
using api_security.domain.Entities.Users;

namespace api_security.infrastructure
{
    public static class DependencyInyection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplication()
                .AddPersistence(configuration);
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

            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
