using api_security.domain.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace api_security.application.Users.Create;

public record CreateUserCommand(string UserName, Guid PatientId, string Password, string Role) : IRequest<Result<Guid>>;


