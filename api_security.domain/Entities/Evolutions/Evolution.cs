using api_security.domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.domain.Entities.Evolutions
{
    public class Evolution : Entity
    {
        public Guid HistoryId { get; private set; }
        public DateTime RegisterDate { get; private set; }
        public string Description { get; private set; }
        public string? Observations { get; private set; }
        public string MedicOrder { get; private set; }

        public Evolution(Guid id, Guid historyId, string description, string observations, string medicOrder)
            : base(id)
        {
            if (historyId == Guid.Empty)
                throw new ArgumentException("El ID de la historia no puede estar vacío", nameof(historyId));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("La descripción no puede estar vacía", nameof(description));
            if (string.IsNullOrWhiteSpace(medicOrder))
                throw new ArgumentException("La orden médica no puede estar vacía", nameof(medicOrder));
            HistoryId = historyId;
            RegisterDate = DateTime.Now.ToUniversalTime();
            Description = description;
            Observations = observations;
            MedicOrder = medicOrder;
        }

        public void Update(string description, string observations, string medicOrder)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("La descripción no puede estar vacía", nameof(description));
            if (string.IsNullOrWhiteSpace(medicOrder))
                throw new ArgumentException("La orden médica no puede estar vacía", nameof(medicOrder));
            RegisterDate = DateTime.Now.ToUniversalTime();
            Description = description;
            Observations = observations;
            MedicOrder = medicOrder;
        }

        private Evolution() : base() { }
    }
}
