using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    public interface ICondicionService
    {

        /// <summary>
        /// Listo todas las Condicion que hay en el sistema (base + creadas por el admin)
        /// </summary>
        /// <returns></returns>
        Task<List<Condicion>> ListarCondicionesAsync();

    }
}
