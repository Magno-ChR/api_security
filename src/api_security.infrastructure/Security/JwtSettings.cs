using System;
using System.Collections.Generic;
using System.Text;

namespace api_security.infrastructure.Security;

public class JwtSettings
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string Secret { get; set; } = default!;
    public int ExpiryMinutes { get; set; }
}
