using api_security.domain.Abstractions;
using api_security.domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.domain.Entities.Patients
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Task UpdateAsync(Patient patient);

        Task<PagedResult<Patient>> GetPagedAsync(int page, int pageSize, string? search = null);

    }
}
