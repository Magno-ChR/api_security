using System.Diagnostics;
using api_security.Extensions;
using api_security.infrastructure;
using api_security.infrastructure.External.Consul;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();

    var lokiUri = context.Configuration["Loki:Uri"];
    if (Uri.TryCreate(lokiUri, UriKind.Absolute, out _))
    {
        loggerConfiguration.WriteTo.GrafanaLoki(lokiUri);
    }
});

builder.Services.AddControllers();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddConsulServiceDiscovery(builder.Configuration);
builder.Services.AddHealthChecks();

var otlpEndpoint = builder.Configuration["Telemetry:OtlpEndpoint"];
if (!string.IsNullOrWhiteSpace(otlpEndpoint) && Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var otlpUri))
{
    var serviceName = builder.Configuration["Telemetry:ServiceName"] ?? "api-security";
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(rb => rb.AddService(serviceName))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(o => o.Endpoint = otlpUri))
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter(o => o.Endpoint = otlpUri));
}

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
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "Json Web Token"
            }
        };

        document.Components.SecuritySchemes = securitySchemes;

        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "API Security";
        options.Theme = ScalarTheme.Purple;
    });
}

app.UseSerilogRequestLogging();

app.UseCors();
app.UseHttpsRedirection();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => true
});

app.Use(async (context, next) =>
{
    var path = $"{context.Request.Path}{context.Request.QueryString}";
    app.Logger.LogInformation(
        "Solicitud HTTP {Method} {Path}",
        context.Request.Method,
        path);

    var sw = Stopwatch.StartNew();
    try
    {
        await next();
    }
    finally
    {
        sw.Stop();
        app.Logger.LogInformation(
            "Respuesta HTTP {Method} {Path} -> {StatusCode} ({ElapsedMs} ms)",
            context.Request.Method,
            path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }
});

app.ApplyMigrations();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Iniciando API Security");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La API terminó de forma inesperada");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
