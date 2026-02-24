using api_security.infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOutboxBackgroundService(delay: 5000);

var host = builder.Build();
host.Run();
