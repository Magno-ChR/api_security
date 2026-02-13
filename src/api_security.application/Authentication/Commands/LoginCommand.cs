using api_security.domain.Results;
using MediatR;


namespace api_security.application.Authentication.Commands;

public record LoginCommand(
    string Username,
    string Password) : IRequest<Result<string>>;
