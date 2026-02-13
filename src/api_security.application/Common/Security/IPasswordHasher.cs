using System;
using System.Collections.Generic;
using System.Text;

namespace api_security.application.Common.Security;

public interface IPasswordHasher
{
    (string hash, string salt) HashPassword(string password);
    string RecalculateHash(string password, string salt);
}
