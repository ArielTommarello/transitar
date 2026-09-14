using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitAR.Structures
{
    /// <summary>
    /// Response de postualcion, tal como el refugio lo ve en su lsitado. Tiene la publicacion, mascota y  datos edel postulante para poder evaluarlo. Aqui se inciai el contacto directo
    /// </summary>
    public class PostulacionRefugioResponse
    {
        /// <summary>
        /// Identificador de la postulacion
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Estado actual de la postulacion
        /// </summary>
        public EstadoPostulacion Estado { get; set; }

        /// <summary>
        /// Disponibilidad en fecha  del postulante para coordinar la visita
        /// </summary>
        public DateTime? DisponibilidadFecha { get; set; }

        /// <summary>
        /// Disponibilidad horaria del postulante
        /// </summary>
        public DisponibilidadHorario? DisponibilidadHorario { get; set; }


        /// <summary>
        /// Fecha en que se postulo
        /// </summary>
        public DateTime FechaPostulacion { get; set; }

        /// <summary>
        /// Fecha en que el refugio la resolvio ,n ull si sigue en evaluacion
        /// </summary>
        public DateTime? FechaResolucion { get; set; }

        /// <summary>
        /// Identificador del postulante, para consultar su historial de tenencias
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Nombre del postulante
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del postulante
        /// </summary>
        public string Apellido { get; set; } = string.Empty;

        /// <summary>
        /// Foto de perfil publica del postulante
        /// </summary>
        public string? FotoUrl { get; set; }

        /// <summary>
        /// Si busca adoptar, transitar o ambas
        /// </summary>
        public SeleccionUsuario Seleccion { get; set; }

        /// <summary>
        /// Tipo de vivienda donde estaria el animal (depto casa, ph)
        /// </summary>
        public string? TipoVivienda { get; set; }

        /// <summary>
        /// Si la vivienda tiene patio
        /// </summary>
        public bool TienePatio { get; set; }

        /// <summary>
        /// Si el patio esta cerrado o es campo (barrios cerrados)
        /// </summary>
        public bool PatioCerrado { get; set; }

        /// <summary>
        /// Si ya tiene otras mascotas en su posesion
        /// </summary>
        public bool TieneOtrasMascotas { get; set; }

        /// <summary>
        /// Detalle de las otras mascotas
        /// </summary>
        public string? DetalleOtrasMascotas { get; set; }

        /// <summary>
        /// Horas por dia que el animal quedaria solo (por trabajao, viaje o demas)
        /// </summary>
        public int? HorasSoloPorDia { get; set; }

        /// <summary>
        /// Cercania a una veterinaria (util en caso de trasnito)
        /// </summary>
        public string? CercaniaVeterinaria { get; set; }

        /// <summary>
        /// Experiencia previa con animales
        /// </summary>
        public string? ExperienciaPrevia { get; set; }

        /// <summary>
        /// Por que busca adoptar o transitar (breve descripcion)
        /// </summary>
        public string? MotivoPostulacion { get; set; }

        /// <summary>
        /// Fecha en que completo su perfil
        /// </summary>
        public DateTime FechaCompletado { get; set; }

        /// <summary>
        /// Redes del postulante para su evaluacion
        /// </summary>
        public List<ContactoResponse> Contactos { get; set; } = new();


    }
}
