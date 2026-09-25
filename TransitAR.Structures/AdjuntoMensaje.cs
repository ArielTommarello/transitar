using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Archivo adjunto para mensaje de chat, contenido guardado en la base. Uso una tabla aparte para mayor portabilidad y no hacer pesados los mensajes.
    /// </summary>
    public class AdjuntoMensaje
    {
        /// <summary>
        /// Identificador del adjunto
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Mensaje al que se le pone este adjunto
        /// </summary>
        public Guid MensajeId { get; set; }

        /// <summary>
        /// Navegacion al mensaje, para el control de acceso al descargarlo (EFCORE)
        /// </summary>
        public Mensaje? Mensaje { get; set; }

        /// <summary>
        /// Nombre original del archivo, para mostrarlo y para la descarga
        /// </summary>
        [Required]
        [MaxLength(260)]
        public string NombreArchivo { get; set; } = string.Empty;

        /// <summary>
        /// Tipo MIME (image/jpeg, video/mp4) distingue foto de video
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Tamaño en bytes, se guarda para poder mostrarlo en la lista sin cargar el contenido
        /// </summary>
        public long TamanioBytes { get; set; }

        /// <summary>
        /// Contenido del archivo
        /// </summary>
        [Required]
        public byte[] Contenido { get; set; } = Array.Empty<byte>();


    }
}
