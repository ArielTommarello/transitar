using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransitAR.Structures;
using Microsoft.Data.Sqlite;

namespace TransitAR.Tests.Soporte
{

    /// <summary>
    /// Base Sqlite en memoria para pruebas (Microsoft Documentation)
    /// </summary>
    public abstract class BaseEnMemoria : IDisposable
    {
        private readonly SqliteConnection _conexion;
        private readonly DbContextOptions<TransitARContext> _opciones;

        protected BaseEnMemoria()
        {
            //la base anda con esta conexion
            _conexion = new SqliteConnection("Filename=:memory:");
            _conexion.Open();

            _opciones = new DbContextOptionsBuilder<TransitARContext>()
                .UseSqlite(_conexion)
                .Options;

            using var contexto = new TransitARContext(_opciones);
            contexto.Database.EnsureCreated();
        }

        /// <summary>
        /// Nuevo contexto creado sobre la base
        /// </summary>
        /// <returns></returns>
        protected TransitARContext CrearContexto() => new TransitARContext(_opciones);


        /// <summary>
        /// Uso en xUnit para cerrar conxion y borrar base
        /// </summary>
        public void Dispose() => _conexion.Dispose();
    }
}
