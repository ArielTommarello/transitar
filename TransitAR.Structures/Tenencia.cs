
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{
    /// <summary>
    /// Tenencia de una mascota con una persona. Se crea cuando el refugio confirma la entrega, no al aceptar la postulacion.
    /// Puede ser tenencia temporal (en caso de transito) o Tenencia en principio final si se adopta. (Si el psotulante lo devuelve la tenencia debe ser actualizada)
    /// Esto permite el seguimiento de la Mascota. Una teniaca con FechaFinReal en null quiere decir que la mascota se encuentra entregada en algun lugar. 
    /// </summary>
    [Index(nameof(PostulacionId), IsUnique =true)]  
    public class Tenencia
    {
        /// <summary>
        /// Identificador de la tenencia
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Postulacion de la que se deriva esta Tenencia
        /// </summary>
        public Guid PostulacionId { get; set; }

        /// <summary>
        /// Mascota que se entrego. Esta repetido en la  cadena Postulacion - Publicacion - Mascota, pero permite armar el historial del animal  y la agenda del refugio con una sola consulta (uso mas facil de linq)
        /// </summary>
        public Guid MascotaId { get; set; }

        /// <summary>
        /// Fecha en que el refugio entrego el animal
        /// </summary>
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fecha pactada de devolucion en un transito. Si el plazo quedo abierto va en null ,no es para adopciones
        /// </summary>
        public DateTime? FechaFinEstimada { get; set; }

        /// <summary>
        /// Fecha en que el animal volvio al refugio. Mientras esta en null el animal sigue ahi (entregado)
        /// </summary>
        public DateTime? FechaFinReal { get; set; }

        /// <summary>
        /// Fecha en que un transito paso a ser adopcion definitiva. No se crea una tenencia nueva porque el animal nunca se movio de esa casa: lo que cambio es el estado en la relacion con el postulante.
        /// Si viene en null, no hubo conversion
        /// </summary>
        public DateTime? FechaConversion { get; set; }

        /// <summary>
        /// observacion por parte del refugio al cerrar la tenencia
        /// </summary>
        [MaxLength(1000)]
        public string? ObservacionCierre { get; set; }

        /// <summary>
        /// Balance de la estadia segun el refugio. Es lo unico que deja antecedente en el perfil de la person. Un transito devuelto de buena manera va en true (Nos sirve para filtrar postulantes)
        /// </summary>
        public bool? FinalizoBien { get; set; }

        /// <summary>
        /// TIpo de tenencia si es transito o adopcion. Nace copiando el Tipo de la publicacion, y cuando un transito se convierte pasa a adopcion.
        /// Sin este campo, una tenencia con FechaFinReal en null no se distingue entre un transito en curso y una adopcion vigente, y la agenda del refugio mostraria como "transito vencido" a una familia que adopto hace dos años
        /// </summary>
        public TipoPublicacion Modalidad { get; set; }

        /// <summary>
        /// Seguimiento de la mascota, en base a esta tenencia
        /// </summary>
        public List<Seguimiento> Seguimientos { get; set; } = new();



    }
}
