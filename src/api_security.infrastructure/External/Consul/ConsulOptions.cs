namespace api_security.infrastructure.External.Consul;

public sealed class ConsulOptions
{
    public const string SectionName = "Consul";

    /// <summary>URL del agente Consul, p. ej. http://INFRA_HOST:8500</summary>
    public string Host { get; set; } = string.Empty;

    public string ServiceName { get; set; } = "api-security";

    /// <summary>IP o hostname desde el que Consul puede alcanzar la API (p. ej. IP del droplet).</summary>
    public string ServiceAddress { get; set; } = "localhost";

    public int ServicePort { get; set; } = 5001;

    public string[] Tags { get; set; } = ["dotnet", "api", "security", "metrics"];

    public string HealthCheckEndpoint { get; set; } = "/health/live";
}
