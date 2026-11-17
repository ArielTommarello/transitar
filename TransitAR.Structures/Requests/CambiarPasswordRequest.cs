using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Datos para que el usuario logueado cambie la contraseña
    /// </summary>
    public class CambiarPasswordRequest
    {

        /// <summary>
        /// Contraseña en texto actual, se compara con el hash guardado
        /// </summary>
        [Required]
        public string PasswordActual { get; set; } = String.Empty;

        /// <summary>
        /// Contraseña en texto nueva, luego se hashea con BCrypt
        /// </summary>
        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string PasswordNueva { get; set; } = String.Empty;


    }
}
