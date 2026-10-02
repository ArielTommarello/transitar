using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    public class RefugioAdminResponse
    {
        /// <summary>
        /// Identificador del refugio
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del refugio
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Mail de contacto del refugio
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Localidad
        /// </summary>
        public string? Localidad { get; set; }

        /// <summary>
        /// Si esta habilitado (bloqueado: sus usuarios no entran y sus publicaciones no se ven)
        /// </summary>
        public bool Activo { get; set; }

        /// <summary>
        /// CFecha de alta del refugio
        /// </summary>
        public DateTime FechaAlta { get; set; }
    }
}
