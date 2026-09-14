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



        //Metodos para RefugioPostulacion (son los que permien que el refugio vea la postulacion a su publicacion)

        /// <summary>
        /// Lista todas las  postulaciones recibidas en una publicacion del refugio 
        /// </summary>
        Task<List<PostulacionRefugioResponse>> ListarPostulacionesDePublicacionAsync(Guid publicacionId, Guid refugioId);

        /// <summary>
        /// Acepta un candidato, las demas pasan a  "en espera" y la publicacion queda pausada hasta que se resuelva el destino de la mascota
        /// </summary>
        Task<PostulacionResult> AceptarAsync(Guid publicacionId, Guid postulacionId, Guid refugioId);

        /// <summary>
        /// Rechaza un candidato , Si era el aceptado, deshace lo hecho  y reabre la publicacion para poder aceptar a otro
        /// </summary>
        Task<PostulacionResult> RechazarAsync(Guid publicacionId, Guid postulacionId, string? observacion, Guid refugioId);
    }

}
