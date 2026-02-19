using api_security.domain.Results;
using MediatR;

namespace api_security.application.Users.Get;

public record GetUserQuery(Guid UserId) : IRequest<Result<GetUserResponse>>;
