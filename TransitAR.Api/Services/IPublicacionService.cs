using TransitAR.Structures;


namespace TransitAR.Api.Services
{
    public interface IPublicacionService
    {
       /// <summary>
       /// Listo todas las publicaciones del refugio
       /// </summary>
       /// <param name="refugioId"></param>
       /// <returns></returns>
        Task<List<PublicacionResponse>> ListarPublicacionesAsync(Guid refugioId);

        /// <summary>
        /// Se obtienen las publicaciones por id y Que sean parte del refugio
        /// </summary>
        /// <param name="id"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<PublicacionResponse?> ObtenerPublicacionAsync(Guid id, Guid refugioId);

        /// <summary>
        /// Crea la publicacion desde el refugio , oslo una por mascota ,ahora devuelve el motivo de error
        /// </summary>
        /// <param name="request"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<PublicacionResult> CrearPublicacionAsync(PublicacionRequest request, Guid refugioId);


        /// <summary>
        /// Actualiza la publicacion, si hay nuevos datos o algo, ahora devuelve el motivo de error
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<PublicacionResult> ActualizarPublicacionAsync(Guid id, PublicacionRequest request, Guid refugioId);


        /// <summary>
        /// Cambia el estado de la publicacion , ahora devuelve el motivo de error
        /// </summary>
        /// <param name="id"></param>
        /// <param name="estado"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<PublicacionResult> CambiarEstadoAsync(Guid id, EstadoPublicacion estado, Guid refugioId);

        //VISTA PUBLICA
        
        /// <summary>
        /// Obtiene las publicaciones activas para el postulante
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<PublicacionPublicaResponse?> ObtenerActivaAsync(Guid id);


        //para la visual de usuario
        /// <summary>
        /// Lista las publicaciones activas de todos los refugios ,todos los filtros son opcionales (usado para que los usuarios peudan ver todas las ofetas)
        /// </summary>
        Task<List<PublicacionPublicaResponse>> ListarActivasAsync(TipoPublicacion? tipo, Guid? especieId,Tamanio? tamanio,Sexo? sexo,string? ubicacion);
    }
}
