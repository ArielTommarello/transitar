
using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{

    /// <summary>
    ///  Tipo de especie del animal , no es raza. Administrado por el Admin (Perro, gato, huron etc). Basado en Especie de AdoptAR
    /// </summary>

    public class Especie
    {
        /// <summary>
        /// identificador de la especie
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre de la especie que se muestra al postulante
        /// </summary>
        [Required]
        [MaxLength(60)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// descripcion adicional para la especie
        /// </summary>
        [MaxLength(150)]
        public string? Descripcion { get; set; } 

    }
}
