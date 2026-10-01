using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System.Net;
using TransitAR.Structures;

namespace TransitAR.Api.Services
{

    /// <summary>
    /// Implementacion de econdiones de la mascota
    /// </summary>
    public class CondicionService : ICondicionService
    {

        private readonly TransitARContext _context;

        /// <summary>
        /// Inicializa el contexto
        /// </summary>
        /// <param name="context"></param>
        public CondicionService(TransitARContext context)
        {
            _context = context;
        }


        ///<inheritdoc/>

        public async Task<List<CondicionResponse>> ListarCondicionesAsync()
        {
            var condiciones = await _context.Condiciones
                .AsNoTracking()
                .OrderBy(c  => c.Nombre)
                .ToListAsync();

            return condiciones.Select(CondicionDTO).ToList();
        }

        ///<inheritdoc/>
        public async Task<CondicionResult> CrearCondicionAsync(CondicionRequest request)
        {
            if (request == null)
                return Error("No hay datos de la especie");
            var nombre = request.Nombre.Trim();

            // para evitar duplicados por nombre. No se podria por id porque tenemos Guid, pero evito que no haya dos perros en todos lados ejemplo.
            if (await _context.Condiciones.AnyAsync(c => c.Nombre == nombre))
                return Error($"Ya existe una condicion llamada {nombre}.");

            var condicion = new Condicion
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = request.Descripcion?.Trim()
            };

            _context.Condiciones.Add(condicion);
            await _context.SaveChangesAsync();

            return new CondicionResult { Condicion = CondicionDTO(condicion) };
        }

        ///<inheritdoc/>
        public async Task<CondicionResult> ActualizarCondicionAsync(Guid id, CondicionRequest request)
        {
            if (request == null || id == Guid.Empty)
                return Error("No hay datos de la especie");

            var condicion = await _context.Condiciones.FirstOrDefaultAsync(c => c.Id == id);

            //no existe la condcion
            if (condicion == null)
                return Error("No encontramos esa condicion.");

            var nombre = request.Nombre.Trim();

            ////para evitar duplicados por nombre. No se podria por id porque tenemos Guid, pero evito que no haya dos condciiones en todos lados ejemplo.
            if (await _context.Condiciones.AnyAsync(c => c.Nombre == nombre && c.Id != id))
                return Error($"Ya existe otra condicion llamada {nombre}.");

            condicion.Nombre = nombre;
            condicion.Descripcion = request.Descripcion?.Trim();

            await _context.SaveChangesAsync();

            return new CondicionResult { Condicion = CondicionDTO(condicion) };
        }

        ///<inheritdoc/>
        public async Task<string?> EliminarCondicionAsync(Guid id)
        {
            var condicion = await _context.Condiciones.FirstOrDefaultAsync(c => c.Id == id);

            if (condicion == null)
                return "No encontramos esa condicion.";

            //evito el DeleteBehavior que me tira SQL y no rompo postualciones activas con esa condicion
            if (await _context.Mascotas.AnyAsync(m => m.CondicionId == id))
                return "No se puede borrar: hay mascotas cargadas con esta condicion.";

            _context.Condiciones.Remove(condicion);
            await _context.SaveChangesAsync();

            return null;
        }


        /// <summary>
        /// Resultado con error
        /// </summary>
        private static CondicionResult Error(string mensaje) => new() { Error = mensaje };

        /// <summary>
        /// Pasa la entidad al DTO
        /// </summary>
        private static CondicionResponse CondicionDTO(Condicion c) => new()
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Descripcion = c.Descripcion
        };

    }
}
