using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Datos que necesita el refugio para registrarse. Se crea el usuario (fundador) , el refugio con sus datos y la relacion entre el usuario/refugio.
    /// El fundador luego puede dar el alta al resto de los usaurios que lo asistiran
    /// </summary>
    public class RegistroRefugioRequest
    {

        /// <summary>
        /// Email del usuario que iniciara sesion en el refugio
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
        /// Nombre del usuario que se registrara como fundador del refugio(requerido)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = String.Empty;


        /// <summary>
        /// Apellido del usuario que se registrara como fundador del refugio (Requerido)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; } = String.Empty;

        /// <summary>
        /// Telefono de contacto
        /// </summary>
        public string? Telefono { get; set; }


        /// <summary>
        /// Nombre del refugio , de esta manera s elo identificara en la plataforma
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string NombreRefugio { get; set; } = String.Empty;


        /// <summary>
        /// Localidad aproximada donde funciona el refugio
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string Localidad{ get; set; } = String.Empty;


        /// <summary>
        /// Domicilio real donde funciona el refugio
        /// </summary>        
        [MaxLength(100)]
        public string? Direccion { get; set; } 

    }
}
