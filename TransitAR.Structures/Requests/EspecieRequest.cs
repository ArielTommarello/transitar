using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Datos para poder cerar una especie y editarla. Agregado para que lo pueda usar el admin
    /// </summary>
    public class EspecieRequest
    {
        /// <summary>
        /// Nombre de la especie (Perro, Gato, Huron)
        /// </summary>
        [Required]
        [MaxLength(60)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion del animal
        /// </summary>
        [MaxLength(150)]
        public string? Descripcion { get; set; }
    }
}
