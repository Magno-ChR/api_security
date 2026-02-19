namespace api_security.application.Users.Get;

public record GetUserResponse(
    string FullName,
    string Username,
    DateTime CreationDate,
    bool IsActive,
    IReadOnlyList<string> Roles);
