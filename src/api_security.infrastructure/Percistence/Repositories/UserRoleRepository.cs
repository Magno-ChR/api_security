using api_security.domain.Entities.UserRoles;
using api_security.domain.Shared;
using api_security.infrastructure.Percistence.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace api_security.infrastructure.Percistence.Repositories;

internal class UserRoleRepository : IUserRoleRepository
{
    private readonly DomainDbContext _context;

    public UserRoleRepository(DomainDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserRole entity)
    {
        await _context.UserRoles.AddAsync(entity);
    }

    public async Task<UserRole?> GetByUserIdAndRoleAsync(Guid userId, RoleType role, bool readOnly = false)
    {
        var query = _context.UserRoles
            .Where(ur => ur.UserId == userId && ur.Role == role);

        if (readOnly)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<UserRole>> GetActiveByUserIdAsync(Guid userId)
    {
        return await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .ToListAsync();
    }
}
