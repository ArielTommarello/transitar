using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Datos para poder cerar una condicion y editarla. Agregado para que lo pueda usar el admin
    /// </summary>
    public class CondicionRequest
    {
        /// <summary>
        /// Nombre de la condicion (Sano, Tratamiento, etc)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion de la condicion
        /// </summary>
        [MaxLength(150)]
        public string? Descripcion { get; set; }
    }
}
