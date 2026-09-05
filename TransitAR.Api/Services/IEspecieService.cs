using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    public interface IEspecieService
    {


        /// <summary>
        /// Listo todas las especies que hay en el sistema (base + creadas por el admin)
        /// </summary>
        /// <returns></returns>
        Task<List<Especie>> ListarEspeciesAsync();
    }
}
