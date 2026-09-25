using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    public class AdjuntoResponse
    {
        /// <summary>
        /// Identificador del adjunto
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre original del archivo
        /// </summary>
        public string NombreArchivo { get; set; } = string.Empty;

        /// <summary>
        /// Tipo MIME. PAra distinguir el tipo a mostrar
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Tamaño en bytes, para mostrarlo sin tener que descargar el archivo
        /// </summary>
        public long TamanioBytes { get; set; }
    }
}
