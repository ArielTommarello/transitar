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
    public class PerfilResponse
    {


        /// <summary>
        /// Identificador del perfil
        /// </summary>
        public Guid Id { get; set; }


        /// <summary>
        /// Si busca adoptar, transitar o las dos cosas. El servicio valida que sea un valor del enum: no puede quedar sin elegir (0)
        /// </summary>
        
        public SeleccionUsuario Seleccion { get; set; }

        /// <summary>
        /// Foto de perfil publica del postulante
        /// </summary>
        
        public string? FotoUrl { get; set; }

        /// <summary>
        /// Tipo de vivienda donde estaria el anima, distincion en depto ph o casa. (ejemplo gatos van a depto algunos)
        /// </summary>        
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
        public string? DetalleOtrasMascotas { get; set; }

        /// <summary>
        /// Horas por dia que el animal quedaria solo (en caso de trabajar fuera o irse de viaje seguido)
        /// </summary>
        public int? HorasSoloPorDia { get; set; }

        /// <summary>
        /// Cercania a una veterinaria. Toma mas valor en los transitos o adopciones con complicaciones
        /// </summary>        
        public string? CercaniaVeterinaria { get; set; }

        /// <summary>
        /// Experiencia previa con animales, o si es primerizo toma valor en casos de complicaciones o animales mas delicados
        /// </summary>        
        public string? ExperienciaPrevia { get; set; }

        /// <summary>
        /// Por que busca adoptar o transitar , breve descripcion
        /// </summary>       
        public string MotivoPostulacion { get; set; } = string.Empty;

        /// <summary>
        /// Fecha en que se completo el perfil por primera vez
        /// </summary>
        public DateTime FechaCompletado { get; set; }

        /// <summary>
        /// Redes cargadas por el postulante
        /// </summary>
        public List<ContactoResponse> Contactos { get; set; } = new();


    }
}
