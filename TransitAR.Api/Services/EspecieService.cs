using Microsoft.EntityFrameworkCore;
using TransitAR.Structures;

namespace TransitAR.Api.Services
{

    /// <summary>
    /// Implementacion de especies
    /// </summary>
    public class EspecieService :IEspecieService
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
        public async Task<List<Especie>> ListarEspeciesAsync()
        {
            return await _context.Especies
                .AsNoTracking()
                .OrderBy(e=> e.Nombre)
                .ToListAsync();
        }





    }
}
