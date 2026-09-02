using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransitAR.Api.Services;
using TransitAR.Structures;

namespace TransitAR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;


        /// <summary>
        /// Iniciacion del servcio de autenticacion
        /// </summary>
        /// <param name="authService">servicio de registro</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        /// <summary>
        /// Registra un ostulante
        /// </summary>
        /// <param name="request">Datos del formulario en request</param>
        /// <returns>datos basicos de la cuenta</returns>
        [HttpPost("registro/postulante")]
        public async Task<IActionResult> RegistrarPostulante([FromBody] RegistroPostulanteRequest request)
        {
            var usuario = await _authService.RegistrarPostulanteAsync(request);

            if (usuario == null)
                return Conflict(new { mensaje = "Ya existe una cuenta registrada con este mail" });

            return Ok(new { usuario.Id, usuario.Email, usuario.Nombre, usuario.Apellido });

        }

        /// <summary>
        /// Registra un usuario fundador y refugio
        /// </summary>
        /// <param name="request">Datos del formulario en request</param>
        /// <returns>datos basicos de la cuenta</returns>
        [HttpPost("registro/refugio")]
        public async Task<IActionResult> RegistrarRefugio([FromBody] RegistroRefugioRequest request)
        {
            var usuario = await _authService.RegistrarRefugioAsync(request);

            if (usuario == null)
                return Conflict(new { mensaje = "Ya existe una cuenta registrada con este mail" });

            return Ok(new { usuario.Id, usuario.Email, usuario.RefugioId });

        }


        /// <summary>
        /// Inicia sesion y devuelve el token de acceso
        /// </summary>
        /// <param name="request">Email y contraseña</param>
        /// <returns>token, expiracion y los datos del usuario</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);

            if (response == null)
                return Unauthorized(new { mensaje = "Email o contraseña incorrectos." });

            return Ok(response);
        }





    }
}
