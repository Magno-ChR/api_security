namespace api_security.infrastructure.Integration;

internal sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string PatientsExchange { get; set; } = "patients";
    public string PatientsQueue { get; set; } = "api_security.patients";
    public string PatientCreatedRoutingKey { get; set; } = "patient.created";
    public string PatientUpdatedRoutingKey { get; set; } = "patient.updated";
}
