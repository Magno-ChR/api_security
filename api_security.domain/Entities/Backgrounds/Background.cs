using api_security.domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.domain.Entities.Backgrounds
{
    public class Background : Entity
    {
        public Guid HistoryId { get; private set; }
        public DateTime RegisterDate { get; private set; }
        public string Description { get; private set; }

        public Background(Guid id, Guid historyId, string description)
            : base(id)
        {
            if (historyId == Guid.Empty)
                throw new ArgumentException("El ID de la historia no puede estar vacío", nameof(historyId));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("La descripción no puede estar vacía", nameof(description));
            HistoryId = historyId;
            RegisterDate = DateTime.Now.ToUniversalTime();
            Description = description;
        }

        public void Update(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("La descripción no puede estar vacía", nameof(description));

            RegisterDate = DateTime.Now.ToUniversalTime();
            Description = description;
        }

        private Background() : base() { }
    }
}
