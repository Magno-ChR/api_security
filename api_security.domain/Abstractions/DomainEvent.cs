using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.domain.Abstractions
{
    public abstract record class DomainEvent : INotification
    {
        public Guid Id { get; set; }
        public DateTime OccurredOn { get; set; }

        public DomainEvent()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }
}
