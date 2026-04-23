namespace api_security.infrastructure.Integration;

/// <summary>Opciones para consumir eventos de paciente desde RabbitMQ.</summary>
internal sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    /// <summary>VirtualHost del broker (por defecto "/").</summary>
    public string VirtualHost { get; set; } = "/";
    /// <summary>Cola a consumir. Debe existir previamente en RabbitMQ.</summary>
    public string QueueName { get; set; } = "ms-security-queue";
    /// <summary>Exchange topic desde el que api_patient publica los eventos de pacientes.</summary>
    public string ExchangeName { get; set; } = "patients";
    public string PatientCreatedRoutingKey { get; set; } = "patient.created";
    public string PatientUpdatedRoutingKey { get; set; } = "patient.updated";
    /// <summary>Segundos de espera entre reintentos al arrancar si RabbitMQ no está listo. 0 = no reintentar.</summary>
    public int ReconnectDelaySeconds { get; set; } = 5;
    /// <summary>
    /// Tamaño de ráfaga de reintentos consecutivos antes de registrar error y reiniciar el contador (0 = sin límite de ráfaga).
    /// El worker no termina el proceso; solo afecta a la frecuencia de logs cuando hay fallos repetidos.
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = 0;
}
