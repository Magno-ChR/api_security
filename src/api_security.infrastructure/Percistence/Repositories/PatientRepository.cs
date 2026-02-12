using api_security.infrastructure.Percistence.DomainModel;
using Microsoft.EntityFrameworkCore;
using patient.domain.Entities.Contacts;
using patient.domain.Entities.Evolutions;
using patient.domain.Entities.Patients;
using patient.domain.Entities.Patients.Events;
using patient.domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.infrastructure.Percistence.Repositories
{
    //No se necesario que sea publico solo se usará aqui
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
                return await context.Patients
                    .AsNoTracking()
                    .Include("_contacts")
                    .FirstOrDefaultAsync(i => i.Id == id);
            }
            else
            {
                return await context.Patients
                    .Include("_contacts")
                    .FirstOrDefaultAsync(i => i.Id == id);
            }
        }

        public async Task<PagedResult<Patient>> GetPagedAsync(int page, int pageSize, string? search = null)
        {
            var query = context.Patients.AsNoTracking();

            // Optional search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.FirstName.Contains(search) ||
                    p.LastName.Contains(search) ||
                    p.DocumentNumber.Contains(search)
                );
            }

            // Count total
            var totalItems = await query.CountAsync();

            // Pagination
            var items = await query
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Patient>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Items = items
            };
        }


        public Task UpdateAsync(Patient entity)
        {
            // Con la configuración correcta de EF (Owned o HasMany),
            // basta con actualizar el agregado completo. EF detectará
            // inserciones/actualizaciones/eliminaciones en la colección.

            //Se detectan eventos de dominio para añadir los contactos
            var added = entity.DomainEvents.Where(x => x is ContactCreateEvent)
                .Select(e => (ContactCreateEvent)e).ToList();

            foreach (var domainEvent in added)
            {
                var itemToAdd = entity.Contacts.First(c => c.Id == domainEvent.ContactId);
                context.Contacts.Add(itemToAdd);
            }

            context.Patients.Update(entity);
            return Task.CompletedTask;
        }

    }
}
