using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures.Requests
{

    /// <summary>
    /// Mensaje y texto que uno al refugio con el postulacnte en un chat de la postulacion
    /// </summary>
    public class MensajeRequest
    {

        [Required]
        [MaxLength(2000)]
        public string Texto { get; set; } = string.Empty;

    }
}
