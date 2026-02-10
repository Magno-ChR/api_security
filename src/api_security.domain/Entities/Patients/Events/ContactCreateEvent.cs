using api_security.domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.domain.Entities.Patients.Events
{
    public record ContactCreateEvent(Guid ContactId) : DomainEvent;

}
