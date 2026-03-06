namespace api_security.infrastructure.Integration;

/// <summary>Opciones para consumir desde la cola ms-security-queue (definida en ms-infrastructure). No se declaran exchanges ni colas.</summary>
internal sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    /// <summary>VirtualHost del broker (por defecto "/").</summary>
    public string VirtualHost { get; set; } = "/";
    /// <summary>Cola a consumir; ya definida en ms-infrastructure definitions.json. Solo consumir, no declarar.</summary>
    public string QueueName { get; set; } = "ms-security-queue";
    public string PatientCreatedRoutingKey { get; set; } = "patient.created";
    public string PatientUpdatedRoutingKey { get; set; } = "patient.updated";
    /// <summary>Segundos de espera entre reintentos al arrancar si RabbitMQ no está listo. 0 = no reintentar.</summary>
    public int ReconnectDelaySeconds { get; set; } = 5;
    /// <summary>Máximo de reintentos al arrancar (0 = infinitos).</summary>
    public int MaxReconnectAttempts { get; set; } = 0;
}
