using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Integrante del equipo de un refugio (FUndador o colaborador), como se muestra en la pagina EQUIPO
    /// </summary>
    public class MiembroEquipoResponse
    {
        /// <summary>
        /// Guid del miembro del equipo
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del miembro del equipo
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del miembro del equipo
        /// </summary>
        public string Apellido {  get; set; } = string.Empty;

        /// <summary>
        /// Email del miembro del equipo
        /// </summary>
        public string Email {  get; set; } = string.Empty;

        /// <summary>
        /// Telefono del miembro del equipo
        /// </summary>
        public string? Telefono { get; set; }

        /// <summary>
        /// Rol del refugio del miembro
        /// </summary>
        public RolRefugio? RolRefugio { get; set; }

        /// <summary>
        /// Si esta activo
        /// </summary>
        public bool Activo { get; set; }

        /// <summary>
        /// Fecha del alta
        /// </summary>
        public DateTime FechaAlta { get; set; }

    }

}
