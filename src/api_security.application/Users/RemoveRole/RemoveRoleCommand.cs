using api_security.domain.Results;
using MediatR;
using Unit = MediatR.Unit;

namespace api_security.application.Users.RemoveRole;

public record RemoveRoleCommand(Guid UserId, string RoleName) : IRequest<Result<Unit>>;
