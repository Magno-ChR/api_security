using api_security.domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace api_security.domain.Entities.Patients;

public class Patient : AggregateRoot
{
    public string FirstName { get; private set; }
    public string MiddleName { get; private set; }
    public string LastName { get; private set; }
    public string DocumentNumber { get; private set; }

    private Patient() : base() { }
}
