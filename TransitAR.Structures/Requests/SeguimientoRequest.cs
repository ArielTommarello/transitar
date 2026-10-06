using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures.Requests
{
    /// <summary>
    /// Datos necesesarios apra erealizar el seguimiento de una tenencia.
    /// Lo crea un refugio, en pendiente. Luego peude pasar a realizado, vencido reprogramado o cancelado, tiene una observacionq ue es visible para ambas partes
    /// </summary>
    public class SeguimientoRequest
    {
        /// <summary>
        /// Tenencia en curso sobre la que se agenda. Solo cuando lo creo
        /// </summary>
        public Guid? TenenciaId { get; set; }

        /// <summary>
        /// Se crea con la fecha en la que se ahra el control, cambia solo cuando se reporgrama
        /// </summary>
        public DateTime? FechaProgramada { get; set; }

        /// <summary>
        /// Estado en el que se encuentra el seguimiento. Lo creamso en pendiente
        /// </summary>
        public EstadoSeguimiento? Estado {  get; set; }


        /// <summary>
        /// Observacion resultado de que salio de la visita si se realizo. Tambien hay observacion si se cancelo o reprogramo, sirve de ayuda en el porque.
        /// </summary>
        [MaxLength(1000)]
        public string? Observacion { get; set; }

    }
}
