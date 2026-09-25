using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{

    /// <summary>
    /// Archivo con contendio. Para subir el archivo adjunto y para bajarlo. Si lleva los bytes
    /// </summary>
    public class ArchivoAdjunto
    {
        /// <summary>
        /// Nombre original del archivo
        /// </summary>
        public string NombreArchivo { get; set; } = string.Empty;

        /// <summary>
        /// Tipo MIME. para validar contra lo permitido
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Contenido del archivo
        /// </summary>
        public byte[] Contenido { get; set; } = Array.Empty<byte>();
    }
}
