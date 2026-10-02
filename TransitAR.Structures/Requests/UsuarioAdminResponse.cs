using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    public class UsuarioAdminResponse
    {
        /// <summary>
        /// Identificador del usuario
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        public string Apellido {  get; set; } = string.Empty;

        /// <summary>
        /// Email con el que se inciia la sesion el user
        /// </summary>
        public string Email {  get; set; } = string.Empty;

        /// <summary>
        /// Admin, Refugio o Usuario comun
        /// </summary>
        public RolUsuario Rol {  get; set; }

        /// <summary>
        /// Si tiene valor, es porque es un refugio. Null si es postulante
        /// </summary>
        public Guid? RefugioId { get; set; }

        /// <summary>
        /// Si esta activo para inciar sesion
        /// </summary>
        public bool Activo {  get; set; }

        /// <summary>
        /// Fecha que se registro
        /// </summary>
        public DateTime FechaAlta { get; set; }
    }
}
