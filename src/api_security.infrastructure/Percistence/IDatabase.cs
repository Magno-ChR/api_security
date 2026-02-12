using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_security.infrastructure.Percistence
{
    /// <summary>
    /// Sirve para no tener que ejecutar la migración desde la consola
    /// </summary>
    public interface IDatabase : IDisposable
    {
        void Migrate();
    }
}
