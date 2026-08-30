
using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{
    /// <summary>
    /// Aviso que realiza un refugio con tal de conseguir una adopcion o transito para una amscota. Puede haber varias publicaciones en el tiempo sobre una mascota, no varias al mismo tiempo.
    /// </summary>
    public class Publicacion
    {
        /// <summary>
        /// Identificador de la publicacion
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Mascota sobre la que se hace la publicacion
        /// </summary>
        public Guid MascotaId { get; set; }

        /// <summary>
        /// Titulo de la publicacion
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion del animal y de lo que busca el refugio. Basado en los textos puestos en las publicaicones de instagram. Aqui se peude agregar si se adopta con otro hermano, que tiene otra publicacion activa.
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Zona donde esta la mascota, para que el postulante pueda filtrar por cercania en caso de necesitarlo
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string Ubicacion { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de publicaicon (adopcion o trasnito)
        /// </summary>
        public TipoPublicacion Tipo { get; set; }

        /// <summary>
        /// Estado de la publciacion. Pasa a pausada cuando se acepta un postulante y a cerrada cuando se confirma la entrega y activa mientras se realiza la busqueda
        /// </summary>
        public EstadoPublicacion Estado { get; set; }

        /// <summary>
        /// Fotos adicionales cargadas en el aviso, en JSON . Las principales estan en la mascota , es para tener una "actualizacion" 
        /// </summary>
        public string? FotosUrlExtra { get; set; }

        /// <summary>
        /// Duracion estimada del transito en dias. Si queda en null se arregla por chat , no aplica a publicaciones de adopcion
        /// </summary>
        public int? PlazoEstimado { get; set; }

        /// <summary>
        /// Fecha en que se publico el aviso
        /// </summary>
        public DateTime FechaPublicacion { get; set; }

        /// <summary>
        /// Fecha en que se cerro el aviso, sea por entrega concretada o por baja. Sirve para usarlo en los seguimientos
        /// </summary>
        public DateTime? FechaCierre { get; set; }

        /// <summary>
        /// Personas que se postularon a este aviso
        /// </summary>
        public List<Postulacion> Postulaciones { get; set; } = new();



    }
}
