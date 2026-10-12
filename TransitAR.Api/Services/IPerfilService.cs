using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    public interface IPerfilService
    {
        /// <summary>
        /// Devuelve el perfil del usuario autenticado, o null si todavia no lo cargo o completo
        /// </summary>
        Task<PerfilResponse?> ObtenerPerfilAsync(Guid usuarioId);

        /// <summary>
        /// Crea el perfil. Devuelve null si el usuario ya tiene uno o paso un error
        /// </summary>
        Task<PerfilResponse?> CrearPerfilAsync(PerfilRequest request, Guid usuarioId);

        /// <summary>
        /// Actualiza el perfil. Devuelve null si el usuario todavia no tiene uno o paso un error
        /// </summary>
        Task<PerfilResponse?> ActualizarPerfilAsync(PerfilRequest request, Guid usuarioId);

        /// <summary>
        /// Guarda o cambia la foto de perfil. Puede no tener una.
        /// </summary>
        /// <param name="usuarioId"></param>
        /// <param name="foto"></param>
        /// <returns></returns>
        Task<bool> CambiarFotoAsync(Guid usuarioId, ArchivoAdjunto foto);

        /// <summary>
        /// Obtiene la foto de perfil , peude no tener una
        /// </summary>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
        Task<ArchivoAdjunto?> ObtenerFotoAsync(Guid usuarioId);

    }
}
