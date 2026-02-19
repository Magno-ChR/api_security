using api_security.domain.Entities.Patients;
using api_security.domain.Entities.Users;
using api_security.domain.Shared;
using api_security.infrastructure.Percistence.DomainModel;
using api_security.infrastructure.Percistence.PersistenceModel.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

    public async Task<User?> GetByIdWithDetailsAsync(Guid id, bool readOnly = false)
    {
        if (readOnly)
        {
            return await context.Users
                .AsNoTracking()
                .Include("_userRoles")
                .FirstOrDefaultAsync(i => i.Id == id);
        }
        else
        {
            return await context.Users
                .Include("_userRoles")
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

    public async Task<User?> GetByPatientIdAsync(Guid patientId, bool readOnly = false)
    {
        if (readOnly)
        {
            return await context.Users
                .AsNoTracking()
                .Include("_credentials")
                .Include("_userRoles")
                .FirstOrDefaultAsync(u => u.PatientId == patientId);
        }
        else
        {
            return await context.Users
                .Include("_credentials")
                .Include("_userRoles")
                .FirstOrDefaultAsync(u => u.PatientId == patientId);
        }
    }

    public async Task<(IReadOnlyList<User> Users, IReadOnlyList<Patient> Patients, int TotalCount)> GetPagedWithPatientsAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        var searchTrimmed = search?.Trim();
        var baseQuery = from u in context.Users.AsNoTracking()
                        join p in context.Patients.AsNoTracking() on u.PatientId equals p.Id
                        where string.IsNullOrWhiteSpace(searchTrimmed) ||
                              p.FirstName.Contains(searchTrimmed!) ||
                              p.MiddleName.Contains(searchTrimmed!) ||
                              p.LastName.Contains(searchTrimmed!) ||
                              p.DocumentNumber.Contains(searchTrimmed!)
                        orderby p.LastName, p.FirstName, u.CreationDate
                        select new { u.Id, u.PatientId };

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var pageData = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (pageData.Count == 0)
            return (Array.Empty<User>(), Array.Empty<Patient>(), totalCount);

        var userIds = pageData.Select(x => x.Id).ToList();
        var patientIds = pageData.Select(x => x.PatientId).ToList();

        var users = await context.Users
            .AsNoTracking()
            .Include("_userRoles")
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(cancellationToken);
        var userDict = users.ToDictionary(u => u.Id);
        var orderedUsers = userIds.Select(id => userDict[id]).ToList();

        var patients = await context.Patients
            .AsNoTracking()
            .Where(p => patientIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
        var patientDict = patients.ToDictionary(p => p.Id);
        var orderedPatients = patientIds.Select(id => patientDict[id]).ToList();

        return (orderedUsers, orderedPatients, totalCount);
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
