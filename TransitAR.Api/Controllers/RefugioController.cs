using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransitAR.Api.Extensions;
using TransitAR.Api.Services;
using TransitAR.Structures;

namespace TransitAR.Api.Controllers
{
    /// <summary>
    /// perfil del refugio. Se identifica por el RefugioId que llega del token. Tiene mensjae de rechazo automatico
    /// </summary>
    

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(RolUsuario.Refugio))]
    public class RefugioController : ControllerBase
    {
        private readonly IRefugioService _refugioService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializa el servicio del perfil del refugio
        /// </summary>
        /// <param name="refugioService"></param>
        public RefugioController(IRefugioService refugioService, IConfiguration configuration)
        {
            _refugioService = refugioService;
            _configuration = configuration;
        }

        /// <summary>
        /// Devuelve el perfil del refugio con sus contactos
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ObtenerPerfil()
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            var perfil = await _refugioService.ObtenerPerfilAsync(refugioId.Value);

            if (perfil == null)
                return NotFound(new { mensaje = "No encontramos el perfil del refugio." });

            return Ok(perfil);
        }

        /// <summary>
        /// Actualiza los datos del perfil del refugio y sincroniza sus contactos
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> ActualizarPerfil([FromBody] RefugioRequest request)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            var contactos = request.Contactos ?? new List<ContactoRequest>();

            //el indice unico de ContactoRefugio no se peude tener dos del mismo tipo
            if (contactos.GroupBy(c => c.Tipo).Any(g => g.Count() > 1))
                return BadRequest(new { mensaje = "No se puede cargar mas de un contacto del mismo tipo." });

            //chequeo del enum contactos tipo
            if (contactos.Any(c => !Enum.IsDefined(c.Tipo)))
                return BadRequest(new { mensaje = "El tipo de contacto indicado no es valido." });

            var perfil = await _refugioService.ActualizarPerfilAsync(request, refugioId.Value);

            if (perfil == null)
                return NotFound(new { mensaje = "No encontramos el perfil del refugio." });

            return Ok(perfil);
        }

        /// <summary>
        /// Sube o cambia la foto de perfil de un refugio
        /// </summary>
        /// <param name="foto"></param>
        /// <returns></returns>
        [HttpPut("foto")]
        public async Task<IActionResult> CambiarFoto(IFormFile foto)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            var maxMB = _configuration.GetValue<int?>("Reglas:FotoPerfilMaxMB") ?? 5;
            var error = FotoPerfilArchivo.Validar(foto, maxMB);
            if (error != null)
                return BadRequest(new { mensaje = error });

            var guardada = await _refugioService.CambiarFotoAsync(refugioId.Value, await FotoPerfilArchivo.LeerAsync(foto));
            if (!guardada)
                return NotFound(new { mensaje = "No encontramos el perfil del refugio." });

            return NoContent();
        }

        /// <summary>
        /// Muestra la foto de perfil del refugio publico
        /// </summary>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        [HttpGet("{refugioId:guid}/foto")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerFoto(Guid refugioId)
        {
            var foto = await _refugioService.ObtenerFotoAsync(refugioId);

            if (foto == null)
                return NotFound();

            return File(foto.Contenido, foto.ContentType);
        }
    }
}
