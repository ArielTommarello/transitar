using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{

    /// <summary>
    /// Conversacion completa, identificada por postulacionId (seria conversacion, las dos partes estan definidads por la postulacion)
    /// </summary>
    public class ChatResponse
    {
        /// <summary>
        /// Postulacion que genrea la conversacion
        /// </summary>
        public Guid PostulacionId { get; set; }

        /// <summary>
        /// Publicacion de la cual
        /// </summary>
        public Guid PublicacionId { get; set; }

        /// <summary>
        /// Titulo de la publicacion
        /// </summary>
        public string PublicacionTitulo { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de publicacion (adopcion o de transito)
        /// </summary>
        public TipoPublicacion Tipo { get; set; }

        /// <summary>
        /// Mascota sobre la conversacion (sirve para filtrar y para no confundir dos chats)
        /// </summary>
        public Guid MascotaId { get; set; }

        /// <summary>
        /// Nombre de la mascota
        /// </summary>
        public string MascotaNombre { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del receptor (el refugio si consulta el postulante, la persona si consulta el refugio)
        /// </summary>
        public string Receptor { get; set; } = string.Empty;

        /// <summary>
        /// Estado actual de la postulacion, define si el chat sigue abierto
        /// </summary>
        public EstadoPostulacion EstadoPostulacion { get; set; }

        /// <summary>
        /// Si todavia se puede escribir. Un chat cerrado queda solo en modo lectura
        /// </summary>
        public bool PuedeEscribir { get; set; }

        /// <summary>
        /// Los mensajes
        /// </summary>
        public List<MensajeResponse> Mensajes { get; set; } = new();

    }
}
