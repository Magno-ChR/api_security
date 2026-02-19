using api_security.domain.Entities.Patients;
using api_security.domain.Entities.Users;
using api_security.domain.Results;
using MediatR;

namespace api_security.application.Users.Get;

public class GetUserHandler : IRequestHandler<GetUserQuery, Result<GetUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPatientRepository _patientRepository;

    public GetUserHandler(IUserRepository userRepository, IPatientRepository patientRepository)
    {
        _userRepository = userRepository;
        _patientRepository = patientRepository;
    }

    public async Task<Result<GetUserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithDetailsAsync(request.UserId, true);
        if (user is null)
            return Result.Failure<GetUserResponse>(Error.NotFound("User.NotFound", "El usuario con ID {id} no existe", request.UserId.ToString()));

        var patient = await _patientRepository.GetByIdAsync(user.PatientId, true);
        var fullName = patient is null
            ? string.Empty
            : $"{patient.FirstName} {patient.MiddleName} {patient.LastName}".Trim();

        var roles = user.UserRoles
            .Where(ur => ur.IsActive)
            .Select(ur => ur.Role.ToString())
            .ToList();

        var response = new GetUserResponse(
            FullName: fullName,
            Username: user.Username,
            CreationDate: user.CreationDate,
            IsActive: user.IsActive,
            Roles: roles);

        return Result.Success(response);
    }
}
