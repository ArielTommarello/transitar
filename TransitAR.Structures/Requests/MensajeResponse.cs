using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    public class MensajeResponse
    {
        /// <summary>
        /// Identificador del mensaje
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Contenido del mensaje ,puede ser vacio si el mensaje es solo un adjunto
        /// </summary>
        public string Texto { get; set; } = string.Empty;

        /// <summary>
        /// Cuando se envio
        /// </summary>
        public DateTime FechaEnvio { get; set; }

        /// <summary>
        /// Cuando lo leyo el otro lado null si no lo leyo
        /// </summary>
        public DateTime? FechaLectura { get; set; }

        /// <summary>
        /// Identifica si el mensaje e spropio, facilita al front
        /// </summary>
        public bool EsPropio { get; set; }

        /// <summary>
        /// Nombre y apellido de quien lo escribio
        /// </summary>
        public string EmisorNombre { get; set; } = string.Empty;

        /// <summary>
        /// Fotos o videos del mensaje, solo datos
        /// </summary>
        public List<AdjuntoResponse> Adjuntos { get; set; } = new();
    }
}
