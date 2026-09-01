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


        [HttpPost("registro/refugio")]
        public async Task<IActionResult> RegistrarREfugio([FromBody] RegistroRefugioRequest request)
        {
            var usuario = await _authService.RegistrarRefugioAsync(request);

            if (usuario == null)
                return Conflict(new { mensaje = "Ya existe una cuenta registrada con este mail" });

            return Ok(new { usuario.Id, usuario.Email, usuario.RefugioId });

        }


    }
}
