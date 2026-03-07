using api_security.infrastructure;

var builder = Host.CreateApplicationBuilder(args);

// Usa las configuraciones propias del proyecto WorkerService (appsettings.json, appsettings.{Environment}.json)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOutboxBackgroundService(delay: 5000);
builder.Services.AddRabbitMqPatientConsumer(builder.Configuration);

var host = builder.Build();
host.Run();
