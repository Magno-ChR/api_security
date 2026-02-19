namespace api_security.application.Users.GetList;

public record PagedUserListResponse(
    IReadOnlyList<UserListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public record UserListItemResponse(
    Guid UserId,
    string FullName,
    string Username,
    string DocumentNumber,
    DateTime CreationDate,
    bool IsActive,
    IReadOnlyList<string> Roles);
