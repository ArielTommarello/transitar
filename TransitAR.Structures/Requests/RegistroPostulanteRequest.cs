using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{

    /// <summary>
    /// Capa de servicio, datos enviados de un postulante para registrarse como adoptante/transitante.
    /// Solo datos de registro, El perfil se realiza despues
    /// </summary>
   public class RegistroPostulanteRequest
    {
        /// <summary>
        /// Email del postulante
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(120)]
        public string Email { get; set; } = String.Empty;


        /// <summary>
        /// Contraseña en texto, luego se hashea con BCrypt
        /// </summary>
        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string Password { get; set; } = String.Empty;


        /// <summary>
        /// Nombre del postulante (requerido)
        /// </summary>
        [Required]
        [MaxLength (100)]
        public string Nombre { get; set; } = String.Empty ;


        /// <summary>
        /// Apellido del postulante (Requerido)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Apellido {  get; set; } = String.Empty ; 

        /// <summary>
        /// Telefono de contacto
        /// </summary>
        public string? Telefono { get; set; }  


    }
}
