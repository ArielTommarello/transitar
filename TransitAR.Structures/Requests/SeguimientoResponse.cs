using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures.Requests
{
    /// <summary>
    /// Datos como lo ven el 
    /// </summary>
    public class SeguimientoResponse
    {
        /// <summary>
        /// Indentificador del seguimiento
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id de la tenencia a la que esta atada el seguimiento
        /// </summary>
        public Guid TenenciaId { get; set; }

        /// <summary>
        /// Id de la mascota a la que se le hace el seguimiento, usado para filtrar
        /// </summary>
        public Guid MascotaId { get; set; }

        /// <summary>
        /// Nombre de la mascota del seguimiento
        /// </summary>
        public string MascotaNombre { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la persona que tiene la tenenica del animal
        /// </summary>
        public string PersonaNombre { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del rfugio que se encarga del seguimiento
        /// </summary>
        public string RefugioNombre { get; set; } = string.Empty;

        /// <summary>
        /// Transito o adopcion
        /// </summary>
        public TipoPublicacion Modalidad { get; set; }

        /// <summary>
        /// Fecha en la que se agendo el control
        /// </summary>
        public DateTime FechaProgramada { get; set; }

        /// <summary>
        /// Fecha en la que se realizo, null si no se hizo
        /// </summary>
        public DateTime? FechaRealizada { get; set; }


        /// <summary>
        /// Observacion que anoto el refugio sobre el camibo de estado (Realizado , cancelado o reporgramado)
        /// </summary>
        public string? Observacion { get; set; }

        /// <summary>
        /// Estado (Realizado, pendiente, Canelado o reprogramado)
        /// </summary>
        public EstadoSeguimiento Estado {  get; set; }

        /// <summary>
        /// Si se paso la fecha de uno pendiente. Lo calculamos, se peude cancelar, reprogramar o marcar realizado.
        /// </summary>
        public bool Vencido { get; set; }
    }
}
