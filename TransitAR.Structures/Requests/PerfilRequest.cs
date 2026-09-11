using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{

    /// <summary>
    /// Datos del postulante, requisitos apra que el perfil este completo para usarse. UsuarioId esta en el token.
    /// Si no se completa este perfil no puede accederse a las funciones para adoptar o transitar
    /// </summary>
    public class PerfilRequest
    {
        /// <summary>
        /// Si busca adoptar, transitar o las dos cosas. El servicio valida que sea un valor del enum: no puede quedar sin elegir (0)
        /// </summary>
        [Required]
        public SeleccionUsuario Seleccion { get; set; }

        /// <summary>
        /// Foto de perfil publica del postulante
        /// </summary>
        [MaxLength(450)]
        public string? FotoUrl { get; set; }

        /// <summary>
        /// Tipo de vivienda donde estaria el anima, distincion en depto ph o casa. (ejemplo gatos van a depto algunos)
        /// </summary>
        [Required]
        [MaxLength(80)]
        public string TipoVivienda { get; set; } = string.Empty;

        /// <summary>
        /// Si la vivienda tiene patio
        /// </summary>
        public bool TienePatio { get; set; }

        /// <summary>
        /// Si el patio esta cerrado
        /// </summary>
        public bool PatioCerrado { get; set; }

        /// <summary>
        /// Si ya convive con otros animales
        /// </summary>
        public bool TieneOtrasMascotas { get; set; }

        /// <summary>
        /// Detalle de las otras mascotas, para una evaluacion mas al detalle
        /// </summary>
        [MaxLength(400)]
        public string? DetalleOtrasMascotas { get; set; }

        /// <summary>
        /// Horas por dia que el animal quedaria solo (en caso de trabajar fuera o irse de viaje seguido)
        /// </summary>
        [Range(0, 24)]
        public int? HorasSoloPorDia { get; set; }

        /// <summary>
        /// Cercania a una veterinaria. Toma mas valor en los transitos o adopciones con complicaciones
        /// </summary>
        [MaxLength(250)]
        public string? CercaniaVeterinaria { get; set; }

        /// <summary>
        /// Experiencia previa con animales, o si es primerizo toma valor en casos de complicaciones o animales mas delicados
        /// </summary>
        [MaxLength(800)]
        public string? ExperienciaPrevia { get; set; }

        /// <summary>
        /// Por que busca adoptar o transitar , breve descripcion
        /// </summary>
        [Required]
        [MaxLength(800)]
        public string MotivoPostulacion { get; set; } = string.Empty;

        /// <summary>
        /// Redes que comparte para que el refugio pueda revisarlas.
        /// </summary>
        public List<ContactoRequest> Contactos { get; set; } = new();
    }
}
