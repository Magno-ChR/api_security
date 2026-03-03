using System.Text.Json.Serialization;

namespace api_security.infrastructure.Integration;

/// <summary>DTO del evento de integración (patient.created / patient.updated). Compatible con el payload del otro servicio.</summary>
internal sealed class PatientIntegrationEventDto
{
    [JsonPropertyName("PatientId")]
    public Guid PatientId { get; set; }

    [JsonPropertyName("Id")]
    public Guid Id { get; set; }

    [JsonPropertyName("OccurredOn")]
    public DateTime OccurredOn { get; set; }

    [JsonPropertyName("FirstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("MiddleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("LastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("DocumentNumber")]
    public string? DocumentNumber { get; set; }
}
