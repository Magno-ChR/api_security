using api_security.application.Common.Security;
using api_security.domain.Abstractions;
using api_security.domain.Entities.Credentials;
using api_security.domain.Entities.UserRoles;
using api_security.domain.Entities.Users;
using api_security.domain.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace api_security.application.Users.Create;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User(Guid.NewGuid(), request.PatientId, request.Password);

        var (hash, salt) = _passwordHasher.HashPassword(request.Password);

        user.AddCredential(new Credential(Guid.NewGuid(), user.Id, hash, salt, DateTime.Now.AddYears(1)));
        user.AddUserRole(new UserRole(Guid.NewGuid(), user.Id, request.RoleType));

        await _userRepository.AddAsync(user);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
