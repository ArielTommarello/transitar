using Microsoft.EntityFrameworkCore;
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

        public async Task<List<Condicion>> ListarCondicionesAsync()
        {
            return await _context.Condiciones
                .AsNoTracking()
                .OrderBy(e => e.Nombre)
                .ToListAsync();
        }


    }
}
