using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures.Requests
{
    /// <summary>
    /// Resultado de una operacion de tenencia. Si tenenica esta null, el error explica porque no se pudo hacer
    /// </summary>
    public class TenenciaResult
    {

        /// <summary>
        /// La tenencia resultante, Null si la operacion no se pudo completar o hubo error
        /// </summary>
        public TenenciaResponse? Tenencia { get; set; }

        /// <summary>
        /// Motivo por el que no se pudo completar o tuvo error, null si salio bien y sino tiene el motivo
        /// </summary>
        public string? Error { get; set; }
    }
}
