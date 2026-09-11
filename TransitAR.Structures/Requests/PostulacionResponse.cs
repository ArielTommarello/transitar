using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures.Requests
{
    /// <summary>
    /// Response de postualcion, tal como el postulante lo ve en su lsitado. Tiene la publicacion, mascota y refugio apra poder filtrar.
    /// </summary>
    public class PostulacionResponse
    {
        /// <summary>
        /// Identificador de la postulacion
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Estado actual (pendiente, en espera, aceptada, rechazada o retirada)
        /// </summary>
        public EstadoPostulacion Estado { get; set; }

        /// <summary>
        /// Fecha que el postulante puso para la visita
        /// </summary>
        public DateTime? DisponibilidadFecha { get; set; }

        /// <summary>
        /// Disponibilidad horaria que puso el postulante
        /// </summary>
        public DisponibilidadHorario? DisponibilidadHorario { get; set; }

        /// <summary>
        /// Fecha en la que se postulo
        /// </summary>
        public DateTime FechaPostulacion { get; set; }

        /// <summary>
        /// Fecha en que el refugio la acepto o rechazo. Null mientras esta pendiente
        /// </summary>
        public DateTime? FechaResolucion { get; set; }

        /// <summary>
        /// Motivo del rechazo : el que escribio el refugio, o su mensaje automatico, o la leyenda por defecto. Null si no fue rechazada
        /// </summary>
        public string? MotivoRechazo { get; set; }

        /// <summary>
        /// Publicacion a la que se postulo
        /// </summary>
        public Guid PublicacionId { get; set; }

        /// <summary>
        /// Titulo de la publicacion
        /// </summary>
        public string PublicacionTitulo { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de publicacion (adopcion o transito)
        /// </summary>
        public TipoPublicacion Tipo { get; set; }

        /// <summary>
        /// Nombre de la mascota
        /// </summary>
        public string MascotaNombre { get; set; } = string.Empty;

        /// <summary>
        /// Especie de la mascota
        /// </summary>
        public string EspecieNombre { get; set; } = string.Empty;

        /// <summary>
        /// Fotos principales de la mascota en JSON, para la tarjeta del listado
        /// </summary>
        public string? FotosUrl { get; set; }

        /// <summary>
        /// Nombre del refugio que publico
        /// </summary>
        public string RefugioNombre { get; set; } = string.Empty;


    }
}
