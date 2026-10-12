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
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializa el servicio de perfil
        /// </summary>
        /// <param name="perfilService"></param>
        public PerfilController(IPerfilService perfilService, IConfiguration configuration)
        {
            _perfilService = perfilService;
            _configuration = configuration; 
        }

        /// <summary>
        /// Devuelve el perfil del usuario 
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerPerfil()
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId == null)
                return Forbid();

            var perfil = await _perfilService.ObtenerPerfilAsync(usuarioId.Value);

            if (perfil == null)
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
            if (usuarioId == null)
                return Forbid();

            var error = ValidarRequest(request);
            if (error != null)
                return BadRequest(new { mensaje = error });

            var perfil = await _perfilService.CrearPerfilAsync(request, usuarioId.Value);

            if (perfil == null)
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
            if (usuarioId == null)
                return Forbid();

            var error = ValidarRequest(request);
            if (error != null)
                return BadRequest(new { mensaje = error });

            var perfil = await _perfilService.ActualizarPerfilAsync(request, usuarioId.Value);

            if (perfil == null)
                return NotFound(new { mensaje = "Todavia no completaste tu perfil. Por favor crealo y completalo para poder postularte" });

            return Ok(perfil);
        }

        /// <summary>
        /// Validaciones internas        
        /// </summary>
        /// <param name="request"></param>
        private static string? ValidarRequest(PerfilRequest request)
        {
            var contactos = request.Contactos ?? new List<ContactoRequest>();

            if (!Enum.IsDefined(request.Seleccion))
                return "Debes seleccionar una opcion: adoptar, transitar o ambas.";

            if (request.DisponibilidadHorario.HasValue && !Enum.IsDefined(request.DisponibilidadHorario.Value))
                return "La disponibilidad horaria indicada no es valida.";

            if (contactos.Any(c => !Enum.IsDefined(c.Tipo)))
                return "Alguna de las redes cargadas tiene un error o tipo no correcto";

            if (contactos.Select(c => c.Tipo).Distinct().Count() != contactos.Count)
                return "No se puede cargar dos veces la misma red social. Por favor eliminala o modifica la enterior";

            return null;
        }

        /// <summary>
        /// Sube o cambia una foto de perfil del postulante
        /// </summary>
        /// <param name="foto"></param>
        /// <returns></returns>

        [HttpPut("foto")]
        public async Task<IActionResult> CambiarFoto(IFormFile foto)
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId == null)
                return Forbid();

            var maxMB = _configuration.GetValue<int?>("Reglas:FotoPerfilMaxMB") ?? 5;
            var error = FotoPerfilArchivo.Validar(foto, maxMB);
            if (error != null)
                return BadRequest(new { mensaje = error });

            var guardada = await _perfilService.CambiarFotoAsync(usuarioId.Value, await FotoPerfilArchivo.LeerAsync(foto));
            if (!guardada)
                return NotFound(new { mensaje = "Debes completar tu perfil antes de subir una foto." });

            return NoContent();
        }

        /// <summary>
        /// Devuevle la foto de perfil de un psotulante publico. Uso allowanonymous para la img y que no se tenga que suar la authorize para el perfil
        /// </summary>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
        [HttpGet("{usuarioId:guid}/foto")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerFoto(Guid usuarioId)
        {
            var foto = await _perfilService.ObtenerFotoAsync(usuarioId);

            if (foto == null)
                return NotFound();

            return File(foto.Contenido, foto.ContentType);
        }
    }
}
