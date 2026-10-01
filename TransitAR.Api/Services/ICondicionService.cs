using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    public interface ICondicionService
    {

        /// <summary>
        /// Listo todas las Condicion que hay en el sistema (base + creadas por el admin)
        /// </summary>
        /// <returns></returns>
        Task<List<CondicionResponse>> ListarCondicionesAsync();

        /// <summary>
        /// Crea una condicion nueva (Solo admin)
        /// </summary>
        Task<CondicionResult> CrearCondicionAsync(CondicionRequest request);

        /// <summary>
        /// Edita nombre o descripcion de una condicion (Solo admin)
        /// </summary>
        Task<CondicionResult> ActualizarCondicionAsync(Guid id, CondicionRequest request);

        /// <summary>
        /// Borra o elimina una condcion(Solo Admin y siempre y cuando nadie lo use)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<string?> EliminarCondicionAsync(Guid id);

    }
}
