using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{
    /// <summary>
    /// Organizacion o refugio, se encarga de cargar las mascotas, publicar los anuncios de transito y adopcion y gestionar las postulaciones
    /// Cada refugio solo puede ver y administrar sus propios recursos.
    /// </summary>
    public class Refugio
    {
        /// <summary>
        /// identificador de la organizacion o refugio
        /// </summary>
        public Guid Id { get; set; }


        /// <summary>
        /// Nombre con el que se muestra el refugio 
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Presentacion y descripcion del refugio para su perfil publico        
        /// </summary>
        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Logo publico del refugio
        /// </summary>
        [MaxLength(450)]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Email con el que se registro y usa de contacto con la pagina el refugio.
        /// </summary>
        [Required]
        [MaxLength(120)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Telefono de contacto del refugio
        /// </summary>
        [MaxLength(80)]
        public string? Telefono { get; set; }

        /// <summary>
        /// Domicilio del refugio
        /// </summary>
        [MaxLength(200)]
        public string? Direccion { get; set; }

        /// <summary>
        /// Localidad donde funciona el refugio
        /// </summary>
        [MaxLength(150)]
        public string? Localidad { get; set; }

        /// <summary>
        /// Texto que se le envia al postulñante en caso de no haber sido seleccionado (No es rechazo, se selecciono otro candidato). Si esta vacio usa el texto por defecto.
        /// </summary>
        [MaxLength(800)]
        public string? MensajeRechazoAutomatico { get; set; }

        /// <summary>
        /// Permite dar de baja un refugio, pero no eliminarlo enc aso de reactivacion o necesidad de que su historial perdure (mascotas y publicaciones)
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// fecha en la que se dio de alta el refugio en nuestra plataforma
        /// </summary>
        public DateTime FechaAlta { get; set; }

        /// <summary>
        /// Redes y links de contacto cargador por el refugio
        /// </summary>
        public List<ContactoRefugio> Contactos { get; set; } = new();

        /// <summary>
        /// lista de mascotas que pertenecen o pertenecieron al refugio
        /// </summary>
        public List<Mascota> Mascotas { get; set; } = new();


    }
}
