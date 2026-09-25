using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Resultado del envio de un mensaje. Diferente tipos de error (PostulacionResult y TenenciaResult)
    /// </summary>
    public class MensajeResult
    {
        /// <summary>
        /// El mensaje creado (null si fallo)
        /// </summary>
        public MensajeResponse? Mensaje { get; set; }

        /// <summary>
        /// Motivo de la falla (null si salio bien)
        /// </summary>
        public string? Error { get; set; }
    }
}
