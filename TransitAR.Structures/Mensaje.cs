

using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{
    /// <summary>
    /// Mensaje del chat interno entre el refugio y el postulante. La conversacion se abre en la postulacion y no en la publicacion, esto es por cada postulante tenga un canal de contacto al mismo tiempo
    /// El chat queda abierto si fue aceptado y la postulacion cerro, de esta manera peude seguir la conversacion en el periodo de tenencia tambien,
    /// </summary>

    public class Mensaje
    {

        /// <summary>
        /// Identificador del mensaje
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Postulacion a la que pertenece la conversacion
        /// </summary>
        public Guid PostulacionId { get; set; }

        /// <summary>
        /// Usuario que envio el mensaje. Puede ser el postulante o la persona del refugio
        /// </summary>
        public Guid EmisorId { get; set; }

        /// <summary>
        /// Texto del mensaje
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string Texto { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de envio
        /// </summary>
        public DateTime FechaEnvio { get; set; }

        /// <summary>
        /// Indica si el destinatario ya lo leyo
        /// </summary>
        public bool Leido { get; set; }

    }
}
