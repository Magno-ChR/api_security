using api_security.infrastructure;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();

    var lokiUri = builder.Configuration["Loki:Uri"];
    if (Uri.TryCreate(lokiUri, UriKind.Absolute, out _))
    {
        loggerConfiguration.WriteTo.GrafanaLoki(lokiUri);
    }
});

var otlpEndpoint = builder.Configuration["Telemetry:OtlpEndpoint"];
if (!string.IsNullOrWhiteSpace(otlpEndpoint) && Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var otlpUri))
{
    var serviceName = builder.Configuration["Telemetry:ServiceName"] ?? "security-worker";
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(rb => rb.AddService(serviceName))
        .WithTracing(tracing => tracing
            .AddOtlpExporter(o => o.Endpoint = otlpUri))
        .WithMetrics(metrics => metrics
            .AddOtlpExporter(o => o.Endpoint = otlpUri));
}

// Usa las configuraciones propias del proyecto WorkerService (appsettings.json, appsettings.{Environment}.json)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOutboxBackgroundService(delay: 5000);
builder.Services.AddRabbitMqPatientConsumer(builder.Configuration);

var host = builder.Build();

try
{
    Log.Information("Iniciando Security Worker");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "El worker terminó de forma inesperada");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
