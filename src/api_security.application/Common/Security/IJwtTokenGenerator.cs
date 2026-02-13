using System;
using System.Collections.Generic;
using System.Text;

namespace api_security.application.Common.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(
        Guid userId,
        Guid personId,
        IEnumerable<string> roles);
}
