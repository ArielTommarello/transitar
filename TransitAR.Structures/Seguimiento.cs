
using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{
    /// <summary>
    /// Control o seguimiento del refugio sobre una tenencia. Agenda de trabajo para el refugio. 
    /// En transito el seguimiento es casi obligatorio, en adopcion lo puede pautar pocos controles el refugio ,si se desea se peude anotar un control por fuera de los programado y se marca realizado en el momento
    /// </summary>
    public class Seguimiento
    {
        /// <summary>
        /// Identificador del seguimiento
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tenencia sobre la que se hace el control
        /// </summary>
        public Guid TenenciaId { get; set; }

        /// <summary>
        /// Fecha para la que se agendo el control o seguimiento
        /// </summary>
        public DateTime FechaProgramada { get; set; }

        /// <summary>
        /// Fecha en la que se realizo el seguimiento o control. Si esta en null esta en pendiente
        /// </summary>
        public DateTime? FechaRealizada { get; set; }

        /// <summary>
        /// Observacion de parte del refugio sobre el control en puntual. Sirve para dejar registro de un problema sin necesidad de cerrar la tenencia
        /// </summary>
        [MaxLength(1000)]
        public string? Observacion { get; set; }


    }
}
