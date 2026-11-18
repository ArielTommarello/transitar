using TransitAR.Structures.Requests;
using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    /// <summary>
    /// Interfaz para el manejo del perfil del refugio, suma los datos con los que se registro + los que agrega para completar el perfil
    /// </summary>
    public interface IRefugioService
    {
        /// <summary>
        /// Devuelve el perfil del refugio que hace la consulta
        /// </summary>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<RefugioResponse?> ObtenerPerfilAsync(Guid refugioId);

        /// <summary>
        /// Actualiza los datos del prefugio y sincroniza contactos
        /// </summary>
        /// <param name="request"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<RefugioResponse?> ActualizarPerfilAsync(RefugioRequest request, Guid refugioId);


        /// <summary>
        /// Guarda o cambia la foto de perfil. Puede no tener una.
        /// </summary>
        /// <param name="refugioId"></param>
        /// <param name="foto"></param>
        /// <returns></returns>
        Task<bool> CambiarFotoAsync(Guid refugioId, ArchivoAdjunto foto);

        /// <summary>
        /// Obtiene la foto de perfil , peude no tener una
        /// </summary>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<ArchivoAdjunto?> ObtenerFotoAsync(Guid refugioId);


        /// <summary>
        /// Lista el equipo del refugi (fundador arriba y abajo colaboradores). Lo ven todos en el refugio
        /// </summary>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<List<MiembroEquipoResponse>> ListarEquipoAsync(Guid refugioId);

        /// <summary>
        /// Da de alta un colaborador en el refugio, solo puede el fundador
        /// </summary>
        /// <param name="fundadorId"></param>
        /// <param name="refugioId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ColaboradorResult> AgregarColaboradorAsync(Guid fundadorId, Guid refugioId, ColaboradorRequest request);

        /// <summary>
        /// Bloqueaa o desbloquea al colaborador del refugio, solo puede el fundador
        /// </summary>
        /// <param name="fundadorId"></param>
        /// <param name="refugioId"></param>
        /// <param name="colaboradorId"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        Task<string?> CambiarEstadoColaboradorAsync(Guid fundadorId, Guid refugioId, Guid colaboradorId, bool activo);

        /// <summary>
        /// Blanquea la conrtaseña de un colaborador del refugio, solo puede el fundador. (null si da bien o devuelve el motivo del error)
        /// </summary>
        /// <param name="fundadorId"></param>
        /// <param name="refugioId"></param>
        /// <param name="colaboradorId"></param>
        /// <param name="passwordNueva"></param>
        /// <returns></returns>
        Task<string?> BlanquearPasswordColaboradorAsync(Guid fundadorId, Guid refugioId, Guid colaboradorId, string passwordNueva);
    }
}
