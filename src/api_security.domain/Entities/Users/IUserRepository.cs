using api_security.domain.Abstractions;
using api_security.domain.Entities.Patients;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace api_security.domain.Entities.Users;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdWithDetailsAsync(Guid id, bool readOnly = false);
    Task<User?> GetByUsernameAsync(string username, bool readOnly = false);
    Task<User?> GetByPatientIdAsync(Guid patientId, bool readOnly = false);
    Task<(IReadOnlyList<User> Users, IReadOnlyList<Patient> Patients, int TotalCount)> GetPagedWithPatientsAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
}
