using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{ 
    /// <summary>
    /// Mensaje y texto que uno al refugio con el postulacnte en un chat de la postulacion
    /// </summary>
    public class MensajeRequest
    {
        /// <summary>
        /// Contenido del mensaje, peude venir null si tiene trae solo adjuntos
        /// </summary>
        
        [MaxLength(2000)]
        public string? Texto { get; set; } = string.Empty;

    }
}
