using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Result de crear , editar o cambiar el estado de una publicaicon. Null y error
    /// </summary>
    public class PublicacionResult
    {
        /// <summary>
        /// La publicacion result. Null si tuvo un error
        /// </summary>
        public PublicacionResponse? Publicacion { get; set; }

        /// <summary>
        /// Error en si de porque paso
        /// </summary>
        public string? Error { get; set; }

    }
}
