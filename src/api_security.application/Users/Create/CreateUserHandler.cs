using api_security.application.Common.Security;
using api_security.domain.Abstractions;
using api_security.domain.Entities.Credentials;
using api_security.domain.Entities.Patients;
using api_security.domain.Entities.UserRoles;
using api_security.domain.Entities.Users;
using api_security.domain.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace api_security.application.Users.Create;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserHandler(IUserRepository userRepository, IPatientRepository patientRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.PatientId, true);
        if (patient is null)
            return Result.Failure<Guid>(new Error("Patient.NotFound", $"El paciente con ID {request.PatientId} no existe", ErrorType.Validation));

        var existingUser = await _userRepository.GetByPatientIdAsync(request.PatientId);
        if (existingUser is not null)
        {
            var hasRole = existingUser.UserRoles.Any(ur => ur.Role == request.RoleType && ur.IsActive);
            var hasCredential = existingUser.Credentials.Any(c => c.IsActive);
            
            if (hasRole && hasCredential)
                return Result.Failure<Guid>(new Error("User.AlreadyExists", $"El paciente con rol {request.RoleType} ya existe", ErrorType.Validation));
        }

        var user = new User(Guid.NewGuid(), request.PatientId, request.UserName);

        var (hash, salt) = _passwordHasher.HashPassword(request.Password);

        user.AddCredential(new Credential(Guid.NewGuid(), user.Id, hash, salt, DateTime.UtcNow.AddYears(1)));
        user.AddUserRole(new UserRole(Guid.NewGuid(), user.Id, request.RoleType));

        await _userRepository.AddAsync(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
