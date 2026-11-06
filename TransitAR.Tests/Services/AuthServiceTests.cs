using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TransitAR.Api.Services;
using TransitAR.Structures;
using TransitAR.Tests.Soporte;

namespace TransitAR.Tests.Services
{
    public class AuthServiceTests : BaseEnMemoria
    {
        private const string Clave = "claveprueba2026";

        private readonly IConfiguration _configuracion;


        public AuthServiceTests()
        {
            //configuracion paraa firmar token
            _configuracion = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "adminTrasitar2026Test-BaseEnMemoria",
                    ["Jwt:Issuer"] = "TransitAR",
                    ["Jwt:Audience"] = "TransitAR",
                    ["Jwt:ExpirationMinutes"] = "60"
                })
                .Build();
        }

        //Cargo un usaurio para las pruebas
        private Usuario CargarUsuario(bool activo = true, Guid? refugioId = null)
        {
            using var contexto = CrearContexto();

            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = "ana@test.org",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Clave),
                Nombre = "Ana",
                Apellido = "Perez",
                Rol = refugioId == null ? RolUsuario.Usuario : RolUsuario.Refugio,
                RefugioId = refugioId,
                Activo = activo
            };

            contexto.Usuarios.Add(usuario);
            contexto.SaveChanges();

            return usuario;
        }

        //Cargo un Refugio para las pruebas
        private Refugio CargarRefugio(bool activo)
        {
            using var contexto = CrearContexto();

            var refugio = new Refugio { Id = Guid.NewGuid(), Nombre = "Patitas", Email = "patitas@test.org", Localidad = "Lomas", Activo = activo };

            contexto.Refugios.Add(refugio);
            contexto.SaveChanges();

            return refugio;
        }

        //Cargo postualnte para las pruebas
        private static RegistroPostulanteRequest Postulante(string email) => new()
        {
            Email = email,
            Password = Clave,
            Nombre = " Ana ",
            Apellido = " Perez "
        };


        //********* PRUBEAS *************

        /// <summary>
        /// Registro de postulante con clave e email normalizado
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RegistrarPostulante_GuardaEmailNormalizadoYClaveHasheada()
        {
            using var contexto = CrearContexto();
            var servicio = new AuthService(contexto, _configuracion);

            var usuario = await servicio.RegistrarPostulanteAsync(Postulante("  ANA@Test.org "));

            Assert.NotNull(usuario);
            Assert.Equal("ana@test.org", usuario.Email);
            Assert.Equal(RolUsuario.Usuario, usuario.Rol);
            Assert.NotEqual(Clave, usuario.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify(Clave, usuario.PasswordHash));
        }

        /// <summary>
        /// Registro de postulante con un email en uso tiene que dar null + error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RegistrarPostulante_ConEmailEnUso_DevuelveNull()
        {
            CargarUsuario();

            using var contexto = CrearContexto();
            var servicio = new AuthService(contexto, _configuracion);
            var cantidad = await contexto.Usuarios.CountAsync();

            var usuario = await servicio.RegistrarPostulanteAsync(Postulante("ANA@test.org"));

            Assert.Null(usuario);
            Assert.Equal(cantidad, await contexto.Usuarios.CountAsync());
        }

        /// <summary>
        /// Registro de refugio, y crear el fundador con ese mail.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RegistrarRefugio_CreaElRefugioYSuFundador()
        {
            using var contexto = CrearContexto();
            var servicio = new AuthService(contexto, _configuracion);

            var usuario = await servicio.RegistrarRefugioAsync(new RegistroRefugioRequest
            {
                Email = "patitas@test.org",
                Password = Clave,
                Nombre = "Laura",
                Apellido = "Gomez",
                NombreRefugio = "Patitas",
                Localidad = "Lomas"
            });

            Assert.NotNull(usuario);
            Assert.Equal(RolUsuario.Refugio, usuario.Rol);
            Assert.Equal(RolRefugio.Fundador, usuario.RolRefugio);
            Assert.True(await contexto.Refugios.AnyAsync(r => r.Id == usuario.RefugioId));
        }


        /// <summary>
        /// Login correcto , devuevle token con el rol de cada uno (postulante/usuario , refugio/usuario)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Login_ConDatosCorrectos_DevuelveTokenConElRol()
        {
            var usuario = CargarUsuario();

            using var contexto = CrearContexto();
            var servicio = new AuthService(contexto, _configuracion);

            var login = await servicio.LoginAsync(new LoginRequest { Email = " ANA@test.org ", Password = Clave });

            Assert.NotNull(login);
            Assert.Equal(usuario.Id, login.UsuarioId);

            var token = new JwtSecurityTokenHandler().ReadJwtToken(login.Token);
            Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Usuario");
        }

        /// <summary>
        /// Login error password - da null + error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Login_ConClaveIncorrecta_DevuelveNull()
        {
            CargarUsuario();

            using var contexto = CrearContexto();
            var servicio = new AuthService(contexto, _configuracion);

            var login = await servicio.LoginAsync(new LoginRequest { Email = "ana@test.org", Password = "otra" });

            Assert.Null(login);
        }


        /// <summary>
        /// Login con mail no registrado - null + error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Login_ConEmailInexistente_DevuelveNull()
        {
            using var contexto = CrearContexto();
            var servicio = new AuthService(contexto, _configuracion);

            var login = await servicio.LoginAsync(new LoginRequest { Email = "nadie@test.org", Password = Clave });

            Assert.Null(login);
        }


        /// <summary>
        /// Login de usaurio bloqueado por ADMIN - da null + error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Login_DeUsuarioBloqueado_DevuelveNull()
        {
            CargarUsuario(activo: false);

            using var contexto = CrearContexto();
            var servicio = new AuthService(contexto, _configuracion);

            var login = await servicio.LoginAsync(new LoginRequest { Email = "ana@test.org", Password = Clave });

            Assert.Null(login);
        }


        /// <summary>
        /// Login cuenta de refugio bloquado por ADMIN  - da null + error
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Login_DeCuentaConRefugioBloqueado_DevuelveNull()
        {
            var refugio = CargarRefugio(activo: false);
            CargarUsuario(refugioId: refugio.Id);

            using var contexto = CrearContexto();
            var servicio = new AuthService(contexto, _configuracion);

            var login = await servicio.LoginAsync(new LoginRequest { Email = "ana@test.org", Password = Clave });

            Assert.Null(login);
        }

        /// <summary>
        /// Login con cuenta de refugio habilitada da token + refugio ok
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Login_DeCuentaConRefugioActivo_DevuelveElRefugio()
        {
            var refugio = CargarRefugio(activo: true);
            CargarUsuario(refugioId: refugio.Id);

            using var contexto = CrearContexto();
            var servicio = new AuthService(contexto, _configuracion);

            var login = await servicio.LoginAsync(new LoginRequest { Email = "ana@test.org", Password = Clave });

            Assert.NotNull(login);
            Assert.Equal(refugio.Id, login.RefugioId);
        }
    }
}
