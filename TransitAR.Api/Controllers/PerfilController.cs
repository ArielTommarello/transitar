using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransitAR.Api.Extensions;
using TransitAR.Api.Services;
using TransitAR.Structures;

namespace TransitAR.Api.Controllers
{

    /// <summary>
    /// Perfil de requisitos del postulante autenticado. Solo uno por usuario, debe completarlo para postularse o pedir transito
    /// </summary>

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(RolUsuario.Usuario))]
    public class PerfilController : ControllerBase
    {
        private readonly IPerfilService _perfilService;

        /// <summary>
        /// Inicializa el servicio de perfil
        /// </summary>
        /// <param name="perfilService"></param>
        public PerfilController(IPerfilService perfilService)
        {
            _perfilService = perfilService;
        }

        /// <summary>
        /// Devuelve el perfil del usuario 
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerPerfil()
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId is null)
                return Forbid();

            var perfil = await _perfilService.ObtenerPerfilAsync(usuarioId.Value);

            if (perfil is null)
                return NotFound(new { mensaje = "Todavia no completaste tu perfil." });

            return Ok(perfil);
        }

        /// <summary>
        /// Crea el perfil del usuario
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        public async Task<IActionResult> CrearPerfil([FromBody] PerfilRequest request)
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId is null)
                return Forbid();

            var error = ValidarRequest(request);
            if (error is not null)
                return BadRequest(new { mensaje = error });

            var perfil = await _perfilService.CrearPerfilAsync(request, usuarioId.Value);

            if (perfil is null)
                return Conflict(new { mensaje = "Ya hay un perfil cargado. Por favor recuperalo y completalo" });

            return Ok(perfil);
        }

        /// <summary>
        /// Actualiza el perfil del usuario 
        /// </summary>
        /// <param name="request"></param>
        [HttpPut]
        public async Task<IActionResult> ActualizarPerfil([FromBody] PerfilRequest request)
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId is null)
                return Forbid();

            var error = ValidarRequest(request);
            if (error is not null)
                return BadRequest(new { mensaje = error });

            var perfil = await _perfilService.ActualizarPerfilAsync(request, usuarioId.Value);

            if (perfil is null)
                return NotFound(new { mensaje = "Todavia no completaste tu perfil. Por favor crealo y completalo para poder postularte" });

            return Ok(perfil);
        }

        /// <summary>
        /// Validaciones internas        
        /// </summary>
        /// <param name="request"></param>
        private static string? ValidarRequest(PerfilRequest request)
        {
            if (!Enum.IsDefined(request.Seleccion))
                return "Debes seleccionar una opcion: adoptar, transitar o ambas.";

            if (request.Contactos.Any(c => !Enum.IsDefined(c.Tipo)))
                return "Alguna de las redes cargadas tiene un error o tipo no correcto";

            if (request.Contactos.Select(c => c.Tipo).Distinct().Count() != request.Contactos.Count)
                return "No se puede cargar dos veces la misma red social. Por favor eliminala o modifica la enterior";

            return null;
        }


    }
}
