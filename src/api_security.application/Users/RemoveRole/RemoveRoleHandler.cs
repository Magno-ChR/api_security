using api_security.domain.Abstractions;
using api_security.domain.Entities.UserRoles;
using api_security.domain.Results;
using api_security.domain.Shared;
using MediatR;
using Unit = MediatR.Unit;

namespace api_security.application.Users.RemoveRole;

public class RemoveRoleHandler : IRequestHandler<RemoveRoleCommand, Result<Unit>>
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRoleHandler(IUserRoleRepository userRoleRepository, IUnitOfWork unitOfWork)
    {
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RoleName))
            return Result.Failure<Unit>(new Error("Role.InvalidName", "El nombre del rol no puede estar vacío", ErrorType.Validation));

        if (!Enum.TryParse<RoleType>(request.RoleName.Trim(), ignoreCase: true, out var roleType) ||
            !Enum.IsDefined(typeof(RoleType), roleType))
            return Result.Failure<Unit>(new Error("Role.Invalid", "El rol especificado no es válido. Valores permitidos: patient, doctor, admin, delivery", ErrorType.Validation));

        var userRole = await _userRoleRepository.GetByUserIdAndRoleAsync(request.UserId, roleType, readOnly: false);
        if (userRole is null)
            return Result.Failure<Unit>(Error.NotFound("UserRole.NotFound", "No se encontró el rol {0} para el usuario indicado", request.RoleName.Trim()));

        userRole.Deactivate();
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}
