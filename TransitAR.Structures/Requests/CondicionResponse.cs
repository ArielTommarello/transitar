using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Condicion como se ve en la API
    /// </summary>
    public class CondicionResponse
    {
        /// <summary>
        /// Identificador de la Condicion
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre de la Condicion
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripcio de la Condicion
        /// </summary>
        public string? Descripcion { get; set; }
    }
}
