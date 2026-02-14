using api_security.domain.Entities.Patients;
using api_security.infrastructure.Percistence.DomainModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace api_security.infrastructure.Percistence.Repositories;

internal class PatientRepository : IPatientRepository
{
    private readonly DomainDbContext context;

    public PatientRepository(DomainDbContext context)
    {
        this.context = context;
    }

    public async Task AddAsync(Patient entity)
    {
        await context.Patients.AddAsync(entity);
    }

    public async Task<Patient?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        if (readOnly)
        {
            return await context.Patients.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        }
        else
        {
            return await context.Patients.FindAsync(id);
        }
    }
}
