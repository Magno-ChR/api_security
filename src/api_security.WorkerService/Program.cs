using api_security.infrastructure;

var builder = Host.CreateApplicationBuilder(args);

// RabbitMQ (ruta/puerto) se toma de api_security/appsettings.json
builder.Configuration.AddJsonFile(
    Path.Combine(builder.Environment.ContentRootPath, "..", "api_security", "appsettings.json"),
    optional: true,
    reloadOnChange: false);
builder.Configuration.AddJsonFile(
    Path.Combine(builder.Environment.ContentRootPath, "..", "api_security", $"appsettings.{builder.Environment.EnvironmentName}.json"),
    optional: true,
    reloadOnChange: false);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOutboxBackgroundService(delay: 5000);
builder.Services.AddRabbitMqPatientConsumer(builder.Configuration);

var host = builder.Build();
host.Run();
