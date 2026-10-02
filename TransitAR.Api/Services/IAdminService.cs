using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    public interface IAdminService
    {
        /// <summary>
        /// Lista los usuarios del sistema. Cada filtro es opcional para que lo vea el admin
        /// </summary>
        /// <param name="rol"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        Task<List<UsuarioAdminResponse>> ListarUsuariosAsync(RolUsuario? rol, bool? activo);

        /// <summary>
        /// Pone en activo o no activo a un usuario (Solo el admin)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        Task<string?> CambiarEstadoUsuarioAsync(Guid id, bool activo);

        /// <summary>
        /// Lista todos los refugios del sistema
        /// </summary>
        /// <param name="activo"></param>
        /// <returns></returns>
        Task<List<RefugioAdminResponse>> ListarRefugiosAsync(bool? activo);

        /// <summary>
        /// Pone en activo o no activo a un refugio (Solo el admin). Bloquea al refugio y a sus usuarios para que no inicien sesion, y sus publicaciones
        /// </summary>
        /// <param name="id"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        Task<string?> CambiarEstadoRefugioAsync(Guid id, bool activo);
    }
}
