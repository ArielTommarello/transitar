using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    /// <summary>
    /// Interfaz para el manejo de resgitro y obtencion de las masctoas por refugio
    /// </summary>
    public interface IMascotaService
    {
        /// <summary>
        /// Listo todas las masctoas del refugio
        /// </summary>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<List<MascotaResponse>> ListarMascotasAsync(Guid refugioId);

        /// <summary>
        /// Listo una mascota por id, fltrando por el refugio que la creo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<MascotaResponse?> ObtenerMascotaAsync(Guid id, Guid refugioId);

        /// <summary>
        /// Creo una mascota a partir de los datos y asignandola al refugio que la creo y donde estara. Valido que la especie y la condicion sean validas
        /// </summary>
        /// <param name="request"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<MascotaResponse?> CrearMascotaAsync(MascotaRequest request, Guid refugioId);

        /// <summary>
        /// Actualizo una mascota a partir de los datos y verificando que pertenezca al refugio que la creo y donde estara. Valido que la especie y la condicion sean validas
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<MascotaResponse?> ActualizarMascotaAsync(Guid id, MascotaRequest request, Guid refugioId);


        /// <summary>
        /// elimino una mascota verificando el id y que pertenezca el refugio
        /// </summary>
        /// <param name="id"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<bool> EliminarMascotaAsync(Guid id, Guid refugioId);

    }
}
