

using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{
    /// <summary>
    /// Animal cargado en el refugio. Existe a parte de las publicaicones, ya que puede publicarse varias veces y con diferentes fines (adopcion, transito, fue devuelvo y necesita adoptarse nuevamente). El estado muestra
    /// donde esta ahora, y los Tenencias son el historial que permitira rastraerlo y hacerle seguimiento.
    /// </summary>
    public class Mascota
    {
        /// <summary>
        /// Identificador de la mascota
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre de la mascota para ese refugio
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Refugio al que pertenece la mascota
        /// </summary>
        public Guid RefugioId { get; set; }

        /// <summary>
        /// Especie del animal. Sale del catalogo que administra el Admin
        /// </summary>
        public Guid EspecieId { get; set; }

        /// <summary>
        /// Condicion actual. Todas las mascotas tienen una, aunque sea Sano (depende de lo que se cargue en condicion dinamicamente)
        /// </summary>
        public Guid CondicionId { get; set; }

        /// <summary>
        /// Raza del animal, si se conoce o  es una cruza
        /// </summary>
        [MaxLength(150)]
        public string? Raza { get; set; }

        /// <summary>
        /// Sexo del animal . Es un enum
        /// </summary>        
        public Sexo? Sexo { get; set; }

        /// <summary>
        /// Fecha de nacimiento aproximada. EL refugio lo cargaría aproximadamente y el ssitema lo guarda con esta fecha. De esta manera en caso de devolucion transito-Adopcion, la edad sigue actualizandose,
        /// y no necesita de una constante edicion. En caso de no saber la edad, peude ponerse null y se mostrara la leyenda "edad desconocida".
        /// </summary>
        public DateTime? FechaNacimientoAproximada { get; set; }

        /// <summary>
        /// Tamanio del animal. Chico, mediano o grande , es un Enum. Sirve para que el postulante pueda filtrar segun necesidades o eleccion.
        /// </summary>
       
        public Tamanio? Tamanio { get; set; }

        /// <summary>
        /// Indica si tiene el plan de vacunacion al dia
        /// </summary>
        public bool Vacunado { get; set; }

        /// <summary>
        /// Donde esta el animal ahora. Es lo que ve el refugio en su listado, sirve en caso de agregar reportes o realizar seguimientos.
        /// </summary>
        public EstadoMascota Estado { get; set; }

        /// <summary>
        /// Fotos del animal en formato JSON, igual que en AdoptAR. La publicacion puede sumar mas sin modificar estas
        /// </summary>
        public string? FotosUrl { get; set; }

        /// <summary>
        /// Fecha en que el refugio cargo al animal o dio de alta
        /// </summary>
        public DateTime FechaAlta { get; set; }

        /// <summary>
        /// Publicaciones que se hicieron de este animal a lo largo de su historia , sirve para realizar un seguimiento del animal y sus estados
        /// </summary>
        public List<Publicacion> Publicaciones { get; set; } = new();

        /// <summary>
        /// Historial de tenencias del animal: con quien estuvo, desde y hasta cuando
        /// </summary>
        public List<Tenencia> Tenencias { get; set; } = new();




    }
}
