using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{
    public class BlanquearPasswordRequest
    {
        /// <summary>
        /// Contraseña temporal que pone el admin cunado el usuario olvido la suya
        /// </summary>
        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string PasswordNueva { get; set; } = String.Empty;
    }
}
