using api_security.domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.domain.Entities.Contacts
{
    public class Contact : Entity
    {
        public Guid PatientId { get; private set; }
        public string Direction { get; private set; }
        public string Reference { get; private set; }
        public string PhoneNumber { get; private set; }
        public string Floor { get; private set; }
        public string Coords { get; private set; }
        public bool IsActive { get; private set; }

        public Contact(Guid id, Guid patientId, string direction, string reference, string phoneNumber, string floor, string coords)
            : base(id)
        {
            if (patientId == Guid.Empty)
                throw new ArgumentException("El paciente asociado no puede ser vacío", nameof(patientId));

            if (string.IsNullOrWhiteSpace(direction))
                throw new ArgumentException("La dirección no puede estar vacía", nameof(direction));

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("El número de teléfono no puede estar vacío", nameof(phoneNumber));

            if (string.IsNullOrWhiteSpace(floor))
                throw new ArgumentException("El piso no puede estar vacío", nameof(floor));

            if (string.IsNullOrWhiteSpace(coords))
                throw new ArgumentException("Las coordenadas no pueden estar vacías", nameof(coords));


            PatientId = patientId;
            Direction = direction;
            Reference = reference ?? string.Empty;
            PhoneNumber = phoneNumber;
            Floor = floor;
            Coords = coords;
            IsActive = true;
        }

        // Nuevo: método para actualizar un contacto preservando las validaciones del dominio
        public void Update(string direction, string reference, string phoneNumber, string floor, string coords)
        {
            if (string.IsNullOrWhiteSpace(direction))
                throw new ArgumentException("La dirección no puede estar vacía", nameof(direction));

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("El número de teléfono no puede estar vacío", nameof(phoneNumber));

            if (string.IsNullOrWhiteSpace(floor))
                throw new ArgumentException("El piso no puede estar vacío", nameof(floor));

            if (string.IsNullOrWhiteSpace(coords))
                throw new ArgumentException("Las coordenadas no pueden estar vacías", nameof(coords));

            Direction = direction;
            Reference = reference ?? string.Empty;
            PhoneNumber = phoneNumber;
            Floor = floor;
            Coords = coords;
        }

        public void logicDelete()
        {
            IsActive = false;
        }

        private Contact() : base() { }
    }
}
