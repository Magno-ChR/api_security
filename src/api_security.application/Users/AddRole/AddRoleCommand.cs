using api_security.domain.Results;
using MediatR;
using Unit = MediatR.Unit;

namespace api_security.application.Users.AddRole;

public record AddRoleCommand(Guid UserId, string RoleName) : IRequest<Result<Unit>>;
