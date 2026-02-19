using api_security.infrastructure.Percistence;
using api_security.infrastructure.Percistence.DomainModel;
using api_security.infrastructure.Percistence.PersistenceModel;
using api_security.infrastructure.Percistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using api_security.domain.Abstractions;
using api_security.domain.Entities.Users;
using api_security.application;
using api_security.application.Common.Security;
using api_security.infrastructure.Security;
using api_security.domain.Entities.Patients;

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

            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
