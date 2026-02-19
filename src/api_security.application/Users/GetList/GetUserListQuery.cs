using api_security.domain.Results;
using MediatR;

namespace api_security.application.Users.GetList;

public record GetUserListQuery(int Page, int PageSize, string? Search) : IRequest<Result<PagedUserListResponse>>;
