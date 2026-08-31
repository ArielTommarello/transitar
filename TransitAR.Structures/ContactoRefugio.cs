
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{
    /// <summary>
    /// Link de Contacto de un refugio. Se guarda un id junto con un id del refugio para poder agregar mas valores o repeticiones  al refugio. De esta manera se facilita la busqueda en linq
    /// </summary>
    [Index(nameof(RefugioId), nameof(Tipo), IsUnique = true)]
    public class ContactoRefugio
    {
        /// <summary>
        /// Identificador del ContactoRefugio
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Refugio al que pertenece este contacto
        /// </summary>
        public Guid RefugioId { get; set; }


        /// <summary>
        /// Tipo de contacto o red social al que pertenece el link
        /// </summary>
        public TipoContacto Tipo { get; set; }

        /// <summary>
        /// url del contacto, el link funciona para el perfil del refugio
        /// </summary>
        [Required]
        [MaxLength(250)]
        public string Url { get; set; } = string.Empty;
    }
}
