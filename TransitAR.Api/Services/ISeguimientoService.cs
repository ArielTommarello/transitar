using TransitAR.Structures.Requests;
using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    /// <summary>
    /// Libreta con los controles para seguimiento de una tenencia. Lo pueden ver el refugio y la persona que tiene la mascota por medio de la tenencia. La persona solo puede ver
    /// </summary>
    public interface ISeguimientoService
    {
        /// <summary>
        /// Lista toda la lsita de seguimientos o controles para le que lo cosnulta, el refugio ve el de todos los animales, y el postulante solo de sus tenencias
        /// </summary>
        /// <param name="usuarioId"></param>
        /// <param name="refugioId"></param>
        /// <param name="estado"></param>
        /// <param name="vencidos"></param>
        /// <param name="mascotaId"></param>
        /// <returns></returns>
        Task<List<SeguimientoResponse>> ListarAsync(Guid usuarioId, Guid? refugioId, EstadoSeguimiento? estado, bool vencidos, Guid? mascotaId);

        /// <summary>
        /// Crea y agenda un control sobre una tenencia que tiene el refugio. Empeiza pendiente y sin la observacion
        /// </summary>
        /// <param name="request"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<string?> CrearAsync(SeguimientoRequest request, Guid refugioId);

        /// <summary>
        /// Cambia el estado puede realizar la tenencia cancelarla o reporgramarla con una observacion. Si es lo ultimo el control se reprograma, queda guardado y se crea uno nuevo en pendiente asi se tiene un control
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        Task<string?> CambiarEstadoAsync(Guid id, SeguimientoRequest request, Guid refugioId);
    }
}
