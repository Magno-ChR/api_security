using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace api_security.infrastructure.Security
{
    public static class JwtAuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Intentar obtener la clave secreta desde configuración en varios nombres comunes.
            var secret = configuration["Jwt:Secret"] ?? configuration["Jwt:Key"] ?? configuration.GetSection("Jwt")?.GetValue<string>("Secret") ?? string.Empty;
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];

            // Si no hay secreto, no configuramos JWT (evita lanzar en tiempo de compilación/arranque).
            if (string.IsNullOrWhiteSpace(secret))
            {
                return services;
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                    ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = signingKey,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

            return services;
        }
    }
}