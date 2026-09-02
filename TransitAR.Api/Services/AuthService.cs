using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TransitAR.Structures;
namespace TransitAR.Api.Services
{

    /// <summary>
    /// Implemebntacion de registro y autenticacion
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly TransitARContext _context;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializacion del contexto
        /// </summary>
        /// <param name="context"></param>
        public AuthService(TransitARContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
                Email = request.Email.Trim().ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                Rol = RolUsuario.Usuario,
                Telefono = request.Telefono?.Trim(),
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


            var email = request.Email.Trim().ToLowerInvariant();

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
            var emailNormalizado = email.Trim().ToLowerInvariant();
            return await _context.Usuarios.AnyAsync(u => emailNormalizado == u.Email);
        }


        ///<inheritdoc/>
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            if (request == null)
                return null;
            var email = request.Email.Trim().ToLowerInvariant();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => email == u.Email);

            //valido al usuario
            if(usuario == null) return null;                   

            //reviso si coincide la contraseña
            if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash)) return null;

            //valido si esta activo
            if (!usuario.Activo) return null;

            //claims
            var claims = new List<Claim>
            {
            new(JwtRegisteredClaimNames.Sub,usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email,usuario.Email),
            new(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
            new(ClaimTypes.Role,usuario.Rol.ToString()),

            };
            //reviso si el usuario tiene rol de refugio y agrego el id de refugio para trabajar sino es usuario normal
            if(usuario.RefugioId.HasValue)
                claims.Add(new Claim("refugioId",usuario.RefugioId.Value.ToString()));

            //reviso que la clave este config
            var keyString = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Falta la configuracion Jwt:key.");

            //misma clave para verifcar y dfirmar
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

            //credenciales
            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationMinutes"));

            //token con jwt
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiracion,
                signingCredentials: creds);


            //return del login y su data
            return new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                FechaExpiracion = expiracion,
                UsuarioId = usuario.Id,
                Email = usuario.Email,
                Nombre = usuario.Nombre,
                Rol = usuario.Rol,
                RefugioId = usuario.RefugioId
            };


        }

       




    }
}
