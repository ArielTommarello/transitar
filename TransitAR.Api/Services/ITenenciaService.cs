using TransitAR.Structures.Requests;
using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    /// <summary>
    /// Tenencias de los animales o mascotas del refugio, a quien se lo dieron , cuando y como se lo dieron o termino .
    /// </summary>

    public interface ITenenciaService
    {

        /// <summary>
        /// Lista las tenencias de los animales del refugio (orden desc)
        /// </summary>
        Task<List<TenenciaResponse>> ListarTenenciasAsync(Guid refugioId);

        /// <summary>
        /// Devuelve una tenencia de un animal del refugio por id
        /// </summary>
        Task<TenenciaResponse?> ObtenerTenenciaAsync(Guid id, Guid refugioId);

        /// <summary>
        /// Confirma la entrega de una postulacion aceptada crea la tenencia, cierra la publicacion, libera a los que estaban en espera y cambia el estado de la mascota
        /// </summary>
        Task<TenenciaResult> ConfirmarEntregaAsync(TenenciaRequest request, Guid refugioId);

    }
}
