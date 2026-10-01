using Microsoft.EntityFrameworkCore;
using TransitAR.Structures;

namespace TransitAR.Api.Services
{

    /// <summary>
    /// Implementacion de especies
    /// </summary>
    public class EspecieService : IEspecieService
    {

        private readonly TransitARContext _context;

        /// <summary>
        /// Inicializa el contexto
        /// </summary>
        /// <param name="context"></param>
        public EspecieService(TransitARContext context)
        {
            _context = context;
        }


        ///<inheritdoc/>
        public async Task<List<EspecieResponse>> ListarEspeciesAsync()
        {
           var especies = await _context.Especies
                .AsNoTracking()
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return especies.Select(EspecieDTO).ToList();
        }

        ///<inheritdoc/>
        public async Task<EspecieResult> CrearEspecieAsync(EspecieRequest request)
        {
            if (request == null)
                return Error ("No hay datos de la especie");

            var nombre = request.Nombre.Trim();

            //para evitar duplicados por nombre. No se podria por id porque tenemos Guid, pero evito que no haya dos perros en todos lados ejemplo.
            if (await _context.Especies.AnyAsync(e => e.Nombre == nombre))
                return Error($"Ya existe una especie llamada {nombre}.");

            var especie = new Especie
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = request.Descripcion?.Trim()
            };

            _context.Especies.Add(especie);
            await _context.SaveChangesAsync();

            return new EspecieResult { Especie = EspecieDTO(especie) };
        }

        ///<inheritdoc/>
        public async Task<EspecieResult> ActualizarEspecieAsync(Guid id, EspecieRequest request)
        {

            if (request == null || id == Guid.Empty)
                return Error("No hay datos de la especie");

            var especie = await _context.Especies.FirstOrDefaultAsync(e => e.Id == id);

            //no existe la especie.
            if (especie == null)
                return Error("No encontramos esa especie.");

            var nombre = request.Nombre.Trim();

            ////para evitar duplicados por nombre. No se podria por id porque tenemos Guid, pero evito que no haya dos perros en todos lados ejemplo.
            if (await _context.Especies.AnyAsync(e => e.Nombre == nombre && e.Id != id))
                return Error($"Ya existe otra especie llamada {nombre}.");

            //actulizo la especie
            especie.Nombre = nombre;
            especie.Descripcion = request.Descripcion?.Trim();

            await _context.SaveChangesAsync();

            return new EspecieResult { Especie = EspecieDTO(especie) };
        }

        ///<inheritdoc/>
        public async Task<string?> EliminarEspecieAsync(Guid id)
        {
            var especie = await _context.Especies.FirstOrDefaultAsync(e => e.Id == id);

            if (especie == null)
                return "No encontramos esa especie.";

            //evito el DeleteBehavior que me tira SQL y no rompo postualciones activas con esa especie
            if (await _context.Mascotas.AnyAsync(m => m.EspecieId == id))
                return "No se puede borrar: hay mascotas cargadas con esta especie.";

            _context.Especies.Remove(especie);
            await _context.SaveChangesAsync();

            return null;
        }

        /// <summary>
        /// Resultado con error
        /// </summary>
        private static EspecieResult Error(string mensaje) => new() { Error = mensaje };

        /// <summary>
        /// Pasa la entidad al DTO
        /// </summary>
        private static EspecieResponse EspecieDTO(Especie e) => new()
        {
            Id = e.Id,
            Nombre = e.Nombre,
            Descripcion = e.Descripcion
        };


    }
}
