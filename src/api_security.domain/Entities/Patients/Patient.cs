using api_security.domain.Abstractions;

namespace api_security.domain.Entities.Patients;

public class Patient : AggregateRoot
{
    public string FirstName { get; private set; } = string.Empty;
    public string MiddleName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string DocumentNumber { get; private set; } = string.Empty;

    private Patient() : base() { }

    private Patient(Guid id, string firstName, string middleName, string lastName, string documentNumber)
        : base(id)
    {
        FirstName = firstName ?? string.Empty;
        MiddleName = middleName ?? string.Empty;
        LastName = lastName ?? string.Empty;
        DocumentNumber = documentNumber ?? string.Empty;
    }

    /// <summary>Crear paciente (p. ej. desde evento de integración patient.created).</summary>
    public static Patient Create(Guid id, string firstName, string middleName, string lastName, string documentNumber) =>
        new(id, firstName ?? string.Empty, middleName ?? string.Empty, lastName ?? string.Empty, documentNumber ?? string.Empty);

    /// <summary>Actualizar datos desde evento de integración patient.updated.</summary>
    public void UpdateDetails(string firstName, string middleName, string lastName, string documentNumber)
    {
        FirstName = firstName ?? string.Empty;
        MiddleName = middleName ?? string.Empty;
        LastName = lastName ?? string.Empty;
        DocumentNumber = documentNumber ?? string.Empty;
    }
}
