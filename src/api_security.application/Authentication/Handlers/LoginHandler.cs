using api_security.application.Authentication.Commands;
using api_security.application.Common.Security;
using api_security.domain.Entities.Users;
using api_security.domain.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace api_security.application.Authentication.Handlers;

public class LoginHandler : IRequestHandler<LoginCommand, Result<string>>
{
    private readonly IJwtTokenGenerator jwtTokenGenerator;
    private readonly IUserRepository userRepository;
    private readonly IPasswordHasher passwordHasher;

    public LoginHandler(IJwtTokenGenerator jwtTokenGenerator, IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        this.jwtTokenGenerator = jwtTokenGenerator;
        this.userRepository = userRepository;
        this.passwordHasher = passwordHasher;
    }

    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Buscar usuario por username en modo sólo lectura
        var user = await userRepository.GetByUsernameAsync(request.Username, readOnly: true);
        if (user == null)
        {
            var notFound = new Error("Authentication.UserNotFound", "User not found", ErrorType.NotFound);
            return Result.Failure<string>(notFound);
        }

        // Obtener sólo credenciales válidas (activas y no expiradas)
        var validCredential = user.Credentials
            .Where(c => c.IsActive && c.ExpirationDate > DateTime.UtcNow)
            .OrderByDescending(c => c.CreationDate)
            .FirstOrDefault();

        if (validCredential == null)
        {
            var err = new Error("Authentication.NoValidCredential", "No valid credentials found for user", ErrorType.Unauthorized);
            return Result.Failure<string>(err);
        }

        // Validar hash usando la salt almacenada
        // Recalcular hash con la salt mediante IPasswordHasher
        var recalculatedHash = passwordHasher.RecalculateHash(request.Password, validCredential.PasswordSalt);
        // Comparación permanece en el handler
        if (!string.Equals(recalculatedHash, validCredential.PasswordHash, StringComparison.Ordinal))
        {
            var unauthorized = new Error("Authentication.InvalidCredentials", "Invalid username or password", ErrorType.Unauthorized);
            return Result.Failure<string>(unauthorized);
        }

        // Construir roles como strings
        var roles = user.UserRoles.Select(r => r.Role.ToString());

        // Generar token
        var token = jwtTokenGenerator.GenerateToken(user.Id, user.PatientId, roles);

        return Result.Success(token);
    }
}