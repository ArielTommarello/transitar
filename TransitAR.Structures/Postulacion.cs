
using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{

    /// <summary>
    /// Solicitud de un postulante sobre una publicaicon especifica. Basado en Visita en AdoptAR, aunque extendido y con otras caracteristicas.
    /// Ocupa un cupo del postulante mientras esta pendiente. Cuando aceptan la postulacion se habilita el chat interno y la puvliacion pasa a Pausada, la entrega se pacta por el chat, por lo que 
    /// se actualizara y se creara Tenencia cuando se confirme la entrega
    /// </summary>
    
    public class Postulacion
    {
        /// <summary>
        /// Identificador de la postulacion
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Publicacion a la que se postula
        /// </summary>
        public Guid PublicacionId { get; set; }

        /// <summary>
        /// Persona que se postula
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Estado actual de la postulacion. Aceptada, pendiente o rechazada
        /// </summary>
        public EstadoPostulacion Estado { get; set; }

        /// <summary>
        /// Fecha en la que el postulante puede coordinar la visita, ayuda a coordinar de manera mas rapida
        /// </summary>
        public DateTime? DisponibilidadFecha { get; set; }

        /// <summary>
        /// Franja horaria en la que el postulante puede coordinar , para una mejor comunicacion
        /// </summary>
        public DisponibilidadHorario? DisponibilidadHorario { get; set; }

        /// <summary>
        /// Fecha en que la persona se postulo a la publicaicon
        /// </summary>
        public DateTime FechaPostulacion { get; set; }

        /// <summary>
        /// Fecha en que el refugio la acepto o rechazo. Mientras esta Pendiente queda en null
        /// </summary>
        public DateTime? FechaResolucion { get; set; }

        /// <summary>
        /// Motivo por el que el refugio rechazo la postulacion. Si viene en null es porque se selecciono a otro candidato, y se muestra el mensaje configurado por el refugio o la leyenda por defecto del sistema
        /// </summary>
        [MaxLength(500)]
        public string? ObservacionRechazo { get; set; }

        /// <summary>
        /// Chat interno entre el refugio y el postulante sobre esta solicitud. Queda abierta hasta que la mascota fuera entregada
        /// </summary>
        public List<Mensaje> Mensajes { get; set; } = new();

        /// <summary>
        /// Tenencia de la masco que se genero a desde esta postulacion. Si viene en null es porque todavia no se confirmo la entrega, o nunca se concreto la tenencia de la mascota
        /// </summary>
        public Tenencia? Tenencia { get; set; }


    }
}
