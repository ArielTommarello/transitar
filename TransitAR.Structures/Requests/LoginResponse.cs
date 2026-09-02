using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{

    /// <summary>
    /// Salida de los datos de login
    /// </summary>

    public class LoginResponse
    {

        public string Token { get; set; } = string.Empty;
        public DateTime FechaExpiracion { get; set; }

        public Guid UsuarioId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public RolUsuario Rol { get; set; } 

        public Guid? RefugioId { get; set; }

    }
}
