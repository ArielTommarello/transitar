using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{
    /// <summary>
    ///  Estado o condicion del aniumal, antes y despues de la adopcion o transito. Basado en la entidad de AdoptAR.
    /// Todos los animales tienen una. Ejemplo (Sano, En tratamiento, post-Operatorio, recuperacion, falta de un miembro, etc).
    /// </summary>
    public class Condicion
    {
        /// <summary>
        /// Identificador de la condicion
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre de la condicion que se mostrara en la mascota
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;


        /// <summary>
        /// descripcion de la condicion de la mascota.
        /// </summary>
        [MaxLength(150)]        
        public string? Descripcion { get; set; } 


    }
}
