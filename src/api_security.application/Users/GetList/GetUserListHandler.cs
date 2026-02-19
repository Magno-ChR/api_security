using api_security.domain.Entities.Users;
using api_security.domain.Results;
using MediatR;

namespace api_security.application.Users.GetList;

public class GetUserListHandler : IRequestHandler<GetUserListQuery, Result<PagedUserListResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUserListHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedUserListResponse>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
    {
        if (request.Page < 1)
            return Result.Failure<PagedUserListResponse>(new Error("Pagination.InvalidPage", "La página debe ser al menos 1", ErrorType.Validation));
        if (request.PageSize < 1 || request.PageSize > 100)
            return Result.Failure<PagedUserListResponse>(new Error("Pagination.InvalidPageSize", "El tamaño de página debe estar entre 1 y 100", ErrorType.Validation));

        var (users, patients, totalCount) = await _userRepository.GetPagedWithPatientsAsync(
            request.Page,
            request.PageSize,
            request.Search,
            cancellationToken);

        var items = new List<UserListItemResponse>(users.Count);
        for (var i = 0; i < users.Count; i++)
        {
            var user = users[i];
            var patient = patients[i];
            var fullName = $"{patient.FirstName} {patient.MiddleName} {patient.LastName}".Trim();
            var roles = user.UserRoles
                .Where(ur => ur.IsActive)
                .Select(ur => ur.Role.ToString())
                .ToList();
            items.Add(new UserListItemResponse(
                UserId: user.Id,
                FullName: fullName,
                Username: user.Username,
                DocumentNumber: patient.DocumentNumber,
                CreationDate: user.CreationDate,
                IsActive: user.IsActive,
                Roles: roles));
        }

        var totalPages = request.PageSize > 0 ? (int)Math.Ceiling(totalCount / (double)request.PageSize) : 0;
        var response = new PagedUserListResponse(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount,
            TotalPages: totalPages);

        return Result.Success(response);
    }
}
