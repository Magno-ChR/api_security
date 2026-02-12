using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api_security.infrastructure.Percistence;

namespace api_security.Extensions;

public static class MigrationExtension
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
        db.Migrate();
    }
}
