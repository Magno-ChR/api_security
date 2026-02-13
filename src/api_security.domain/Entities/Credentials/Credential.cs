using api_security.domain.Abstractions;


namespace api_security.domain.Entities.Credentials;

public class Credential : Entity
{
    public Guid UserId { get; private set; }
    public string PasswordHash { get; private set; }
    public string PasswordSalt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreationDate { get; private set; }
    public DateTime ExpirationDate { get; private set; }

    public Credential(Guid id, Guid userId, string passwordHash, string passwordSalt, DateTime expirationDate)
        : base(id)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("El ID del usuario no puede estar vacío", nameof(userId));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash de la contraseña no puede estar vacío", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(passwordSalt))
            throw new ArgumentException("La sal de la contraseña no puede estar vacía", nameof(passwordSalt));
        if (expirationDate <= DateTime.UtcNow)
            throw new ArgumentException("La fecha de expiración debe ser futura", nameof(expirationDate));
        UserId = userId;
        IsActive = true;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        CreationDate = DateTime.UtcNow;
        ExpirationDate = expirationDate;
    }

    public Credential Create (Guid id, Guid userId, string passwordHash, string passwordSalt, DateTime expirationDate)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("El ID del usuario no puede estar vacío", nameof(userId));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash de la contraseña no puede estar vacío", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(passwordSalt))
            throw new ArgumentException("La sal de la contraseña no puede estar vacía", nameof(passwordSalt));
        if (expirationDate <= DateTime.UtcNow)
            throw new ArgumentException("La fecha de expiración debe ser futura", nameof(expirationDate));

        return new Credential(id, userId, passwordHash, passwordSalt, expirationDate);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private Credential() : base() { }
}
