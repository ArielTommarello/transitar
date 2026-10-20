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
        /// Borro una mascota que estaba vargada por error, o no se uso, junot a sus fotos. Devuelvo el error directamente.
        /// null si se borro. Si tiene historia (publicaciones o rtenencias) no se puede borrar, pausar publicacion.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<string?> EliminarMascotaAsync(Guid id, Guid refugioId);

        /// <summary>
        /// Masctoa de una tenencia del postulante , para usar en la ficha de la tenencia (null si no tiene)
        /// </summary>
        /// <param name="tenenciaId"></param>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
        Task<MascotaResponse?> ObtenerMascotaDeTenenciaAsync(Guid tenenciaId, Guid usuarioId);



        //FOTOS ADJUNTAS MASCOTA

        /// <summary>
        /// Agrega las fotos de la masctoa (se agregan al final de las que estan , cola)
        /// </summary>
        /// <param name="mascotaId"></param>
        /// <param name="refugioId"></param>
        /// <param name="fotos"></param>
        /// <param name="maximo"></param>
        /// <returns></returns>
        Task<string?> AgregarFotosAsync(Guid mascotaId, Guid refugioId, List<ArchivoAdjunto> fotos, int maximo);

        /// <summary>
        /// Borra una foto y las mueve al lugar que viene .
        /// </summary>
        /// <param name="mascotaId"></param>
        /// <param name="fotoId"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<bool> BorrarFotoAsync(Guid mascotaId, Guid fotoId, Guid refugioId);


        /// <summary>
        /// Hace principal una foto que se elija
        /// </summary>
        /// <param name="mascotaId"></param>
        /// <param name="fotoId"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<bool> HacerPrincipalAsync(Guid mascotaId, Guid fotoId, Guid refugioId);


        /// <summary>
        /// Devuelve la foto para mostrarla
        /// </summary>
        /// <param name="fotoId"></param>
        /// <returns></returns>
        Task<ArchivoAdjunto?> ObtenerFotoAsync(Guid fotoId);
    }
}
