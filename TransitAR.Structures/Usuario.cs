
using System.ComponentModel.DataAnnotations;

namespace TransitAR.Structures
{
    /// <summary>
    /// Usuario registrado en la plataforma, el usuario decide si ser adoptante , transitante o ambos. Usuarios con rol refugio son el personal del mismo que puede utilizarlo
    /// </summary>
    public class Usuario
    {
        /// <summary>
        /// Identificador del usuario
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Email con el que inicia sesion. Es unico en toda la plataforma
        /// </summary>
        [Required]
        [MaxLength(120)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contrasenia hasheada con BCrypt
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la persona detras de la cuenta
        /// En una cuenta de refugio es la persona de contacto, no el refugio en si 
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Apellido de la persona dueña de la cuenta
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        /// <summary>
        /// Telefono de contacto
        /// </summary>
        [MaxLength(80)]
        public string? Telefono { get; set; }

        /// <summary>
        /// Rol que tiene dentro del sistema. Se asigna al registrarse y no se cambia (Refugio o Postulante)
        /// </summary>
        public RolUsuario Rol { get; set; }

        /// <summary>
        /// Nivel dentro del rol refugio.Solo se usa para diferenciar  fundador de colaborador y asi poder usar mas personas en el refugio
        /// </summary>
        public RolRefugio? RolRefugio { get; set; }

        /// <summary>
        /// Solo en cuentas con rol Refugio: indica que refugio administra esta persona.
        /// En cuentas de postulantes y del admin queda en null
        /// </summary>
        public Guid? RefugioId { get; set; }

        /// <summary>
        /// Permite al Admin bloquear una cuenta sin borrar su historial. Una cuenta inactiva no puede iniciar sesion ni postularse , debera hablar con soporte o el admin
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Fecha en que se registro en la plataforma
        /// </summary>
        public DateTime FechaAlta { get; set; }

        /// <summary>
        /// Datos de postulante. Si viene en null, la cuenta todavia no completo su perfil o no es una cuenta de postulante (caso refugio o admin)
        /// </summary>
        public PerfilPostulante? Perfil { get; set; }
    }
}
