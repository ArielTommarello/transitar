using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Especie como se ve en la API
    /// </summary>
    public class EspecieResponse
    {
       /// <summary>
       /// Identificador de la Especie
       /// </summary>
       public Guid Id { get; set; }

      /// <summary>
      /// Nombre de la especie
      /// </summary>
       public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripcio de la especie
        /// </summary>
        public string? Descripcion { get; set; }
    }
}
