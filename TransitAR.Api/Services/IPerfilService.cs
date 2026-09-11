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

    }
}
