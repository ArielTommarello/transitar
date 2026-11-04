using TransitAR.Structures;

namespace TransitAR.Api
{
    /// <summary>
    /// Crea la cuenta del admin al inciar con los datos de la configracion
    /// email en appsetins y clave en user-secrets
    /// </summary>
    public static class AdminInicial
    {

        public static Usuario? Armar(IConfiguration config)
        {
            var email = config["Email"];
            var clave = config["Clave"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(clave))
            {
                Console.WriteLine("AdminInicial: falta Email o Clave en la configuracion, no se crea el admin.");
                return null;
            }

            return new Usuario
            {
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(clave),
                Nombre = config["Nombre"] ?? "Administrador",
                Apellido = config["Apellido"] ?? "TransitAR",
                Rol = RolUsuario.Admin,
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };
        }
    }
}
