using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Datos que carga el fundador al dar de alta un colaborador en su refugio
    /// </summary>
    public class ColaboradorRequest
    {

        /// <summary>
        /// Email con el que logueara el colaborador
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(120)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña inicial que da el fundador y que el colaborador despues cambia
        /// </summary>
        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string Password { get; set; } = String.Empty;

        /// <summary>
        /// Nombre del colaborador
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = String.Empty;


        /// <summary>
        /// Apellido del colaborador
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; } = String.Empty;

        /// <summary>
        /// Telefono del colaborador
        /// </summary>
        public string? Telefono {get; set;} 
    }
}
