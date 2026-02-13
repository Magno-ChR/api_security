using api_security.domain.Entities.Users;
using api_security.domain.Shared;
using api_security.infrastructure.Percistence.DomainModel;
using api_security.infrastructure.Percistence.PersistenceModel.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.infrastructure.Percistence.Repositories;

internal class UserRepository : IUserRepository
{
    private readonly DomainDbContext context;
    public UserRepository(DomainDbContext context)
    {
        this.context = context;
    }
    public async Task AddAsync(User entity)
    {
        await context.Users.AddAsync(entity);
    }
    public async Task<User?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        if (readOnly)
        {
            return await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);
        }
        else
        {
            return await context.Users
                .FirstOrDefaultAsync(i => i.Id == id);
        }
    }

    public async Task<User?> GetByUsernameAsync(string username, bool readOnly = false)
    {
        if (readOnly)
        {
            return await context.Users
                .AsNoTracking()
                .Include("_credentials")
                .Include("_userRoles")
                .FirstOrDefaultAsync(u => u.Username == username);
        }
        else
        {
            return await context.Users
                .Include("_credentials")
                .Include("_userRoles")
                .FirstOrDefaultAsync(u => u.Username == username);
        }
    }
    //public async Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, string? search = null)
    //{
    //    var query = context.Users.AsNoTracking();

    //    // Optional search
    //    if (!string.IsNullOrWhiteSpace(search))
    //    {
    //        query = query.Where(p =>
    //            p.FirstName.Contains(search) ||
    //            p.LastName.Contains(search) ||
    //            p.DocumentNumber.Contains(search)
    //        );
    //    }

    //    // Count total
    //    var totalItems = await query.CountAsync();

    //    // Pagination
    //    var items = await query
    //        .OrderBy(p => p.FirstName)
    //        .ThenBy(p => p.LastName)
    //        .Skip((page - 1) * pageSize)
    //        .Take(pageSize)
    //        .ToListAsync();

    //    return new PagedResult<User>
    //    {
    //        Page = page,
    //        PageSize = pageSize,
    //        TotalItems = totalItems,
    //        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
    //        Items = items
    //    };
    //}
}
