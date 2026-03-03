using MediatR;

namespace api_security.application.Integration.Patients;

/// <summary>Comando para sincronizar un paciente desde eventos de integración (patient.created / patient.updated).</summary>
public sealed class SyncPatientFromIntegrationCommand : IRequest<Unit>
{
    public Guid PatientId { get; init; }
    public bool IsCreated { get; init; }
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? DocumentNumber { get; init; }
}
