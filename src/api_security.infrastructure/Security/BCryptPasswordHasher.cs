using api_security.application.Common.Security;


namespace api_security.infrastructure.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
    public (string hash, string salt) HashPassword(string password)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var hash = BCrypt.Net.BCrypt.HashPassword(password, salt);
        return (hash, salt);
    }
}
