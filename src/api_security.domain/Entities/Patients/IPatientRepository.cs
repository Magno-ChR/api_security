using api_security.domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace api_security.domain.Entities.Patients;

public interface IPatientRepository : IRepository<Patient>
{
}
