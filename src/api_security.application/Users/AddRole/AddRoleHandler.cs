using api_security.domain.Abstractions;
using api_security.domain.Entities.UserRoles;
using api_security.domain.Entities.Users;
using api_security.domain.Results;
using api_security.domain.Shared;
using MediatR;
using Unit = MediatR.Unit;

namespace api_security.application.Users.AddRole;

public class AddRoleHandler : IRequestHandler<AddRoleCommand, Result<Unit>>
{
    private const int MaxRolesPerUser = 3;

    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddRoleHandler(
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Unit>> Handle(AddRoleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RoleName))
            return Result.Failure<Unit>(new Error("Role.InvalidName", "El nombre del rol no puede estar vacío", ErrorType.Validation));

        if (!Enum.TryParse<RoleType>(request.RoleName.Trim(), ignoreCase: true, out var roleType) ||
            !Enum.IsDefined(typeof(RoleType), roleType))
            return Result.Failure<Unit>(new Error("Role.Invalid", "El rol especificado no es válido. Valores permitidos: patient, doctor, admin, delivery", ErrorType.Validation));

        var user = await _userRepository.GetByIdAsync(request.UserId, readOnly: true);
        if (user is null)
            return Result.Failure<Unit>(Error.NotFound("User.NotFound", "El usuario con ID {0} no existe", request.UserId.ToString()));

        var activeRoles = await _userRoleRepository.GetActiveByUserIdAsync(request.UserId);
        if (activeRoles.Count >= MaxRolesPerUser)
            return Result.Failure<Unit>(new Error("UserRole.MaxRoles", $"El usuario ya tiene el máximo de {MaxRolesPerUser} roles asignados", ErrorType.Validation));

        if (activeRoles.Any(r => r.Role == roleType))
            return Result.Failure<Unit>(new Error("UserRole.Duplicate", "El usuario ya tiene asignado el rol " + request.RoleName.Trim(), ErrorType.Conflict));

        var userRole = new UserRole(Guid.NewGuid(), request.UserId, roleType);
        await _userRoleRepository.AddAsync(userRole);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}
