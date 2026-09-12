using TransitAR.Structures.Requests;
using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    public interface IPostulacionService
    {
        /// <summary>
        /// Lista las postulaciones del usuario de la mas nueva a la mas vieja
        /// </summary>
        Task<List<PostulacionResponse>> ListarMisPostulacionesAsync(Guid usuarioId);

        /// <summary>
        /// Devuelve una postulacion del usuario
        /// </summary>
        Task<PostulacionResponse?> ObtenerPostulacionAsync(Guid id, Guid usuarioId);

        /// <summary>
        /// Postula el usuario en una publicacion, validando perfil, cupo y estado de la publicacion
        /// </summary>
        Task<PostulacionResult> PostularseAsync(PostulacionRequest request, Guid usuarioId);

        /// <summary>
        /// Retira una postulacion propia que todavia no fue resuelta par aliberar cupo
        /// </summary>
        Task<PostulacionResult> RetirarPostulacionAsync(Guid id, Guid usuarioId);
    }

}
