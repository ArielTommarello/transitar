using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    public class ChatResumenResponse
    {
        /// <summary>
        /// Postulacion id para identificar la conversacion
        /// </summary>
        public Guid PostulacionId { get; set; }

        /// <summary>
        /// Publicacion de la que se habla par afiltrar
        /// </summary>
        public Guid PublicacionId { get; set; }

        /// <summary>
        /// Mascota de la que se habla para poder filtrar
        /// </summary>
        public Guid MascotaId { get; set; }

        /// <summary>
        /// Nombre de la mascota
        /// </summary>
        public string MascotaNombre { get; set; } = string.Empty;

        /// <summary>
        /// Adopcion o transito. Va en la tarjeta porque la misma mascota puede tener dos chats si se publico dos veces (caso transito-adopcion)
        /// </summary>
        public TipoPublicacion Tipo { get; set; }

        /// <summary>
        /// Nombre del recpetor
        /// </summary>
        public string Receptor { get; set; } = string.Empty;

        /// <summary>
        /// Estado de la postulacion
        /// </summary>
        public EstadoPostulacion EstadoPostulacion { get; set; }

        /// <summary>
        /// Si todavia se puede escribir o el chat esta cerrado modo lectura
        /// </summary>
        public bool PuedeEscribir { get; set; }

        /// <summary>
        /// Texto del ultimo mensaje (null si todavia nadie escribio)
        /// </summary>
        public string? UltimoMensaje { get; set; }

        /// <summary>
        /// Cuando se envio el ultimo mensaje (mull si no hay ninguno)
        /// </summary>
        public DateTime? FechaUltimoMensaje { get; set; }

        /// <summary>
        /// Si el ultimo mensaje trae adjuntos.Para renderizar la preview de un icono para adjunto
        /// </summary>
        public bool UltimoTieneAdjunto { get; set; }

        /// <summary>
        /// Cantidad de mensajes sin leer
        /// </summary>
        public int NoLeidos { get; set; }
    }
}
