using api_security.domain.Abstractions;
using api_security.domain.Shared;

namespace api_security.domain.Entities.UserRoles;

public class UserRole : Entity
{
    public Guid UserId { get; private set; }
    public RoleType Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreationDate { get; private set; }

    public UserRole(Guid id, Guid userId, RoleType role)
        : base(id)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("El ID del usuario no puede estar vacío", nameof(userId));
        if (!Enum.IsDefined(typeof(RoleType), role))
            throw new ArgumentException("El rol no es válido", nameof(role));

        UserId = userId;
        Role = role;
        IsActive = true;
        CreationDate = DateTime.UtcNow;
    }

    public void ChangeRole(RoleType newRole)
    {
        if (!Enum.IsDefined(typeof(RoleType), newRole))
            throw new ArgumentException("El rol no es válido", nameof(newRole));
        Role = newRole;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    } 
    private UserRole() : base() { }
}
