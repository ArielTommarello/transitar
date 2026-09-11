using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{

    /// <summary>
    /// Datos con los que se postula un postulante a una publicacion. Todo lo demas sale del usuario
    /// </summary>
    public class PostulacionRequest
    {
        /// <summary>
        /// Publicacion a la que se postula , tiene que estar activa
        /// </summary>
        public Guid PublicacionId { get; set; }

        /// <summary>
        /// Fecha en la que el postulante puede coordinar la visita o tiene disponibilidad
        /// </summary>
        public DateTime? DisponibilidadFecha { get; set; }

        /// <summary>
        /// Disponibilidad horaria en la que puede coordinar
        /// </summary>
        public DisponibilidadHorario? DisponibilidadHorario { get; set; }

    }
}
