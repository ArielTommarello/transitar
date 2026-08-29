
using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{
    /// <summary>
    /// Link de Contacto de un usuario. Se guarda un id junto con un id del usuario para poder agregar mas valores o repeticiones  al refugio. De esta manera se facilita la busqueda en linq
    /// </summary>
    public class ContactoUsuario
    {
        /// <summary>
        /// Identificador del ContactoUsuario
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Usuarip al que pertenece este contacto
        /// </summary>
        public Guid UsuarioId { get; set; }


        /// <summary>
        /// Tipo de contacto o red social al que pertenece el link
        /// </summary>
        public TipoContacto Tipo { get; set; }

        /// <summary>
        /// url del contacto, el link funciona para el perfil del usuario
        /// </summary>
        [Required]
        [MaxLength(250)]
        public string Url { get; set; } = string.Empty;
    }
}
