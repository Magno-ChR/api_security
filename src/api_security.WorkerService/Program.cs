using api_security.infrastructure;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = Host.CreateApplicationBuilder(args);
var serviceName = builder.Configuration["Telemetry:ServiceName"] ?? "security-worker";

builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();

    var lokiUri = builder.Configuration["Loki:Uri"];
    if (!string.IsNullOrWhiteSpace(lokiUri) &&
        Uri.TryCreate(lokiUri.Trim(), UriKind.Absolute, out var loki) &&
        (loki.Scheme == Uri.UriSchemeHttp || loki.Scheme == Uri.UriSchemeHttps))
    {
        loggerConfiguration.WriteTo.GrafanaLoki(
            lokiUri.Trim(),
            [new LokiLabel { Key = "service_name", Value = serviceName }]);
    }
});

var otlpEndpoint = builder.Configuration["Telemetry:OtlpEndpoint"];
if (!string.IsNullOrWhiteSpace(otlpEndpoint) && Uri.TryCreate(otlpEndpoint.Trim(), UriKind.Absolute, out var otlpUri))
{
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
