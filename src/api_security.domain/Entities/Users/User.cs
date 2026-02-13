using api_security.domain.Abstractions;
using api_security.domain.Entities.Credentials;
using api_security.domain.Entities.UserRoles;

namespace api_security.domain.Entities.Users;

public class User : AggregateRoot
{
    public Guid PatientId { get; private set; }
    public string Username { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreationDate { get; private set; }
    public DateTime UpdateDate { get; private set; }
    public DateTime LastLoginDate { get; private set; }
    public DateTime BlockedUntilDate { get; private set; }
    private readonly List<Credential> _credentials = new();
    public IReadOnlyCollection<Credential> Credentials => _credentials.AsReadOnly();
    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();


    public User(Guid id, Guid patientId, string username)
        : base(id)
    {
        if (patientId == Guid.Empty)
            throw new ArgumentException("El ID del paciente no puede estar vacío", nameof(patientId));
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(username));
        PatientId = patientId;
        Username = username;
        IsActive = true;
        CreationDate = DateTime.UtcNow;
        UpdateDate = DateTime.UtcNow;
        FailedLoginAttempts = 0;
    }

    public User Create(Guid id, Guid patientId, string username)
    {
        if (patientId == Guid.Empty)
            throw new ArgumentException("El ID del paciente no puede estar vacío", nameof(patientId));
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(username));
        return new User(id, patientId, username);
    }

    public void AddCredential(Credential credential)
    {
        if (credential == null)
            throw new ArgumentNullException(nameof(credential), "La credencial no puede ser nula");
        _credentials.Add(credential);
        UpdateDate = DateTime.UtcNow;
    }

    public void AddUserRole(UserRole userRole)
    {
        if (userRole == null)
            throw new ArgumentNullException(nameof(userRole), "El rol de usuario no puede ser nulo");
        _userRoles.Add(userRole);
        UpdateDate = DateTime.UtcNow;
    }
    public void UpdateLastLoginDate()
    {
        LastLoginDate = DateTime.UtcNow;
        UpdateDate = DateTime.UtcNow;
    }

    public void IncrementFailedLoginAttempts()
    {
        FailedLoginAttempts++;
        UpdateDate = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdateDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateDate = DateTime.UtcNow;
    }

    private User() : base() { }
}
