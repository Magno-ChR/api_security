using api_security.Extensions;
using api_security.infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
// Learn más sobre la configuración de OpenAPI en https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi(o =>
{
    o.AddDocumentTransformer((document, _, _) =>
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes.Add(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri("<your-app-auth-endpoint>"),
                    TokenUrl = new Uri("<your-app-token-endpoint>"),
                    Scopes = new Dictionary<string, string>
                    {
                        {"api://<client-id>/data.read", "Read Data"}
                    },
                    // To allow Scalar to select PKCE by Default
                    // valid options are 'SHA-256' | 'plain' | 'no'
                    Extensions = new Dictionary<string, IOpenApiExtension>()
                    {
                        ["x-usePkce"] = new OpenApiString("SHA-256")
                        // Prefill Client Secret using extension below: 
                        // Don't hardcode this value. Instead fetch it from a secure configuration source (local user secrets, Key Vault, etc.)
                        // ["clientSecret"] = new OpenApiString("<your-secret>")
                    }

                }
            }
        });

        // Provide a security requirement for all operations (preselected default security scheme)
        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                // scopes
                ["api://<client-id>/data.read"]
            }
        });

        return Task.CompletedTask;

    });
});

builder.Services.AddJwtAuth(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.ApplyMigrations();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
