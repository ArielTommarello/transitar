using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    public interface IEspecieService
    {


        /// <summary>
        /// Listo todas las especies que hay en el sistema (base + creadas por el admin)
        /// </summary>
        /// <returns></returns>
        Task<List<EspecieResponse>> ListarEspeciesAsync();

        /// <summary>
        /// Crea una especie nueva (Solo admin)
        /// </summary>
        Task<EspecieResult> CrearEspecieAsync(EspecieRequest request);

        /// <summary>
        /// Edita nombre o descripcion de una especie (Solo admin)
        /// </summary>
        Task<EspecieResult> ActualizarEspecieAsync(Guid id, EspecieRequest request);

        /// <summary>
        ///  Borra o elimina una especie (Solo Admin y siempre y cuando nadie lo use)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<string?> EliminarEspecieAsync(Guid id);
    }
}
