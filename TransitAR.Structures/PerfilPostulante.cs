
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{
    /// <summary>
    /// Aqui estan los datos del usuario que desa postularse, el refugio los revisa junto con el historial de tenencias que peude tener. Si no existe , la cuenta no completo requerimientos y se le bloqueara la postulacion hasta entonces.
    /// </summary>
    [Index(nameof(UsuarioId), IsUnique = true)]
   public class PerfilPostulante
    {
        /// <summary>
        /// Identificador del perfil
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Usuario al que pertenece este perfil
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Seleccion del usuario, trasnito , adopcion o ambas. Queda sujeto a cambio por parte del usuario
        /// </summary>
        public SeleccionUsuario Seleccion { get; set; }

        /// <summary>
        /// Foto de perfil publica del usuario
        /// </summary>
        [MaxLength(450)]
        public string? FotoUrl { get; set; }

        /// <summary>
        /// Tipo de vivienda donde estar ael animal en transito o adoptadp
        /// </summary>
        [MaxLength(80)]
        public string? TipoVivienda { get; set; }

        /// <summary>
        /// Requerimiento , si la vivienda tiene patio
        /// </summary>
        public bool TienePatio { get; set; }

        /// <summary>
        /// Requerimiento , si la vivienda tiene patio cerrado
        /// </summary>
        public bool PatioCerrado { get; set; }

        /// <summary>
        /// Requerimiento , si el usuario ya cuenta con otro animal
        /// </summary>
        public bool TieneOtrasMascotas { get; set; }

        /// <summary>
        /// Requerimiento , se despega del requerimiento TieneOtrasMascotas, detalle para una mayor evaluacion exhaustiva
        /// </summary>
        [MaxLength(400)]
        public string? DetalleOtrasMascotas { get; set; }

        /// <summary>
        /// Requerimiento, horas de soleadad de la mascota (por diferentes casos ,trabajo, enfermedad, viaje ,etc)
        /// </summary>
        public int? HorasSoloPorDia { get; set; }

        /// <summary>
        /// Requerimiento , si tiene cercania a una veterinaria, toma mas valor en caso de transitos
        /// </summary>
        [MaxLength(250)]
        public string? CercaniaVeterinaria { get; set; }

        /// <summary>
        /// Requerimiento , si el usuario ya tiene alguna experiencia previa o es primerizo
        /// </summary>
        [MaxLength(800)]
        public string? ExperienciaPrevia { get; set; }

        /// <summary>
        /// Requerimiento , breve descripcion de poeque busca adoptar o transitar
        /// </summary>
        [MaxLength(800)]
        public string? MotivoPostulacion { get; set; }

        /// <summary>
        /// Fecha de completado del perfil
        /// </summary>
        public DateTime FechaCompletado { get; set; }

        /// <summary>
        /// Redes sociales que comparte voluntariamente para que el refugio pueda revisarlas
        /// </summary>
        public List<ContactoPostulante> Contactos { get; set; } = new();
    }
}
