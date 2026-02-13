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

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Security API",
            Version = "v1",
            Description = "API for authentication and authorization"
        };

        document.Components ??= new OpenApiComponents();

        var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer", // "bearer" refers to the header name here
                In = ParameterLocation.Header,
                BearerFormat = "Json Web Token"
            }
        };

        document.Components.SecuritySchemes = securitySchemes;

        return Task.CompletedTask;
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "API Security";
        options.Theme = ScalarTheme.Purple;
    });
}

app.UseHttpsRedirection();
app.ApplyMigrations();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
