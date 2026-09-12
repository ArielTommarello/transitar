using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitAR.Structures.Requests;

namespace TransitAR.Structures
{
    /// <summary>
    /// clase auxiliar que da el resultado de una postulacion, si esta null el error explcia porque. Metodo para poder diferenciar errores en usuario
    /// </summary>
    public class PostulacionResult
    {

        /// <summary>
        /// La postulacion en si, Null si la operacion no se pudo completar
        /// </summary>
        public PostulacionResponse? Postulacion { get; set; }

        /// <summary>
        /// Motivo por el que no se pudo completar ,Null si salio todo ok
        /// </summary>
        public string? Error { get; set; }

    }
}
