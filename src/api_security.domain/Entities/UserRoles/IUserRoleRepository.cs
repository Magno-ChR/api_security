namespace api_security.domain.Entities.UserRoles;

public interface IUserRoleRepository
{
    Task AddAsync(UserRole entity);
    Task<UserRole?> GetByUserIdAndRoleAsync(Guid userId, Shared.RoleType role, bool readOnly = false);
    Task<IReadOnlyList<UserRole>> GetActiveByUserIdAsync(Guid userId);
}
