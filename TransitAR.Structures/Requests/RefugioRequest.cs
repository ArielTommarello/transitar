using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{

    /// <summary>
    /// Datos del perfil del refugio, ya hay un refugio, pero sirven para completar (misma idea que usuario). EL id lo traigo del oken y  fechaAlta y Activo lo maneja el admin y el servidor.
    /// </summary>
    public class RefugioRequest
    {
        /// <summary>
        /// Nombre con el que se muestra el refugio en las publicaciones
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion del refugio para su perfil 
        /// </summary>
        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Logo del refugio
        /// </summary>
        [MaxLength(450)]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Mail de contacto del refugio, el que ve la gente. Es el mail de contacto, no tiene porque ser con el que se registro el refugio, ese esta en usuario
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(120)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Telefono de contacto del refugio
        /// </summary>
        [MaxLength(80)]
        public string? Telefono { get; set; }

        /// <summary>
        /// Direccion del refugio
        /// </summary>
        [MaxLength(200)]
        public string? Direccion { get; set; }

        /// <summary>
        /// Localidad donde esta el refugio
        /// </summary>
        [MaxLength(150)]
        public string? Localidad { get; set; }

        /// <summary>
        /// Texto que se le manda al postulante cuando no fue seleccionado y el refugio no escribio un motivo propio.Si es null se usa el texto por defecto del sistema
        /// </summary>
        [MaxLength(800)]
        public string? MensajeRechazoAutomatico { get; set; }

        /// <summary>
        /// Redes y links del refugio, uno solo por tipo.
        /// </summary>
        public List<ContactoRequest> Contactos { get; set; } = new();



    }
}
