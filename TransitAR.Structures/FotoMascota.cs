using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Foto de la mascota,guardada en la base. Puede tener varias fotos
    /// </summary>
    public class FotoMascota
    {
        /// <summary>
        /// identificador de la foto
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Mascota a la que pertnece la foto
        /// </summary>
        public Guid MascotaId { get; set; }

        /// <summary>
        /// NAvegacion a la mascota (EFCORE)
        /// </summary>
        public Mascota? Mascota { get; set; }

        /// <summary>
        /// Posicion de la foto para mostrar en las tarjetas
        /// </summary>
        public int Orden {  get; set; }

        /// <summary>
        /// Tipo de la imagen (jpeg, png, webp)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;


        /// <summary>
        /// COntenido de la iamgen
        /// </summary>
        [Required]
        public byte[] Contenido { get; set; } = Array.Empty<byte>();

    }
}
