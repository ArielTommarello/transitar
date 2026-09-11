using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Redes o links a redes que el postulante comparte. Se usa para que el refugio pueda revisarlo . solo uno por tipo , armado dinamicamente
    /// </summary>
    public class ContactoRequest
    {
        /// <summary>
        /// Que red es: Web, Instagram, Facebook, WhatsApp o Tiktok.Tiene que ser un valor del enum tipoContacto        
        /// </summary>
        public TipoContacto Tipo { get; set; }

        /// <summary>
        /// Direccion del perfil
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string Url { get; set; } = string.Empty;

    }
}
