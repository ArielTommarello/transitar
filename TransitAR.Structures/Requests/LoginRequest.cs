using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    public class LoginRequest
    {
        /// <summary>
        /// Email del usuario para loguearse
        /// </summary>
        [Required]
        [EmailAddress]        
        public string Email { get; set; } = String.Empty;

        /// <summary>
        /// Contraseña en texto, luego se hashea con BCrypt , esto es para loguearse
        /// </summary>
        [Required]       
        public string Password { get; set; } = String.Empty;



    }
}
