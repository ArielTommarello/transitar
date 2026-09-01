using Microsoft.EntityFrameworkCore;
using TransitAR.Structures;
namespace TransitAR.Api.Services
{

    /// <summary>
    /// Implemebntacion de registro y autenticacion
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly TransitARContext _context;

        /// <summary>
        /// Inicializacion del contexto
        /// </summary>
        /// <param name="context"></param>
        public AuthService(TransitARContext context)
        {
            _context = context;
        }

        ///<inheritdoc/>
        public async Task<Usuario?> RegistrarPostulanteAsync (RegistroPostulanteRequest request)
        {
            if(request == null)
                return null;
            if(await EmailEnUsoAsync(request.Email))
                return null;

            var postulante = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                Rol = RolUsuario.Usuario,
                RolRefugio = null,
                RefugioId = null,
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };


            _context.Usuarios.Add(postulante); 
            await _context.SaveChangesAsync();  
            return postulante;
        }

        ///<inheritdoc/>
        public async Task<Usuario?> RegistrarRefugioAsync(RegistroRefugioRequest request)
        {
            if (request == null)
                return null;
            if (await EmailEnUsoAsync(request.Email))
                return null;


            var email = request.Email.Trim().ToLower();

            var refugio = new Refugio
            {
                Id = Guid.NewGuid(),
                Nombre = request.NombreRefugio.Trim(),
                Email = email,
                Telefono = request.Telefono?.Trim(),
                Localidad = request.Localidad?.Trim(),
                Direccion = request.Direccion?.Trim(),
                Activo = true,
                FechaAlta = DateTime.UtcNow 
            };

            var usuarioRefugio = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                Telefono = request.Telefono?.Trim(),
                Rol = RolUsuario.Refugio,
                RolRefugio = RolRefugio.Fundador,
                RefugioId = refugio.Id,
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };

            _context.Refugios.Add(refugio);
            _context.Usuarios.Add(usuarioRefugio);
            await _context.SaveChangesAsync();
            return usuarioRefugio;
        }


        private async Task<bool> EmailEnUsoAsync(string email)
        {
            var emailNormalizado = email.Trim().ToLower();
            return await _context.Usuarios.AnyAsync(u => emailNormalizado == u.Email);
        }



    }
}
