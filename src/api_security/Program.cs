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
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);
var serviceName = builder.Configuration["Telemetry:ServiceName"] ?? "api-security";

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();

    var lokiUri = context.Configuration["Loki:Uri"];
    if (!string.IsNullOrWhiteSpace(lokiUri) &&
        Uri.TryCreate(lokiUri.Trim(), UriKind.Absolute, out var loki) &&
        (loki.Scheme == Uri.UriSchemeHttp || loki.Scheme == Uri.UriSchemeHttps))
    {
        loggerConfiguration.WriteTo.GrafanaLoki(
            lokiUri.Trim(),
            [new LokiLabel { Key = "service_name", Value = serviceName }]);
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
if (!string.IsNullOrWhiteSpace(otlpEndpoint) && Uri.TryCreate(otlpEndpoint.Trim(), UriKind.Absolute, out var otlpUri))
{
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

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} -> {StatusCode} ({Elapsed:0.0} ms)";
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex is not null || httpContext.Response.StatusCode >= 500)
        {
            return LogEventLevel.Error;
        }

        var path = httpContext.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/metrics", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
        {
            return LogEventLevel.Verbose;
        }

        return LogEventLevel.Information;
    };
});

app.UseCors();
app.UseHttpsRedirection();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => true
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
