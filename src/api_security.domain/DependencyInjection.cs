using api_security.domain.Entities.Patients;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.domain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            //services.AddSingleton<IPatientStrategy, Strategy>
            return services;
        }
    }
}
