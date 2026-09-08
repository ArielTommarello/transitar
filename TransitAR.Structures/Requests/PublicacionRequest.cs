using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Datos que el refugio caraga al crear o editar una publicacion
    /// Estado, FechaPublicaicon y FechadeCierre los pone el sevidor automaticamente. La mascota Id solo se usa al crear o pausar, ya qu eno puede haber mas de un anuncio de esa mascota.
    /// </summary>

    public class PublicacionRequest
    {

        /// <summary>
        /// Mascota sobre la que se hace la publicaicon, tiene que pertenecer al refugio
        /// </summary>
        public Guid MascotaId { get; set; }

        /// <summary>
        /// Titulo de la publicaicon
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Descripcion de la mascota y los requisitos que quiere el refugio
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Zona donde esta la mascota, para que el postulante filtre por cercania (Generalemtne es el refugio)
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string Ubicacion { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de Aviso (Adopcion o Transito)
        /// </summary>
        public TipoPublicacion Tipo { get; set; }

        /// <summary>
        /// Fotos adicionales del aviso en JSON. Las principales estan en la mascota
        /// </summary>
        public string? FotosUrlExtra { get; set; }

        /// <summary>
        /// Duracion estimada del transito en dias , no cuenta si es adopcion
        /// </summary>
        public int? PlazoEstimado { get; set; }


    }
}
