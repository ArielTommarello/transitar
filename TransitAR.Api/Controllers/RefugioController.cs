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
                return BadRequest(new { mensaje = "No se puede cargar más de un contacto del mismo tipo." });

            //chequeo del enum contactos tipo
            if (contactos.Any(c => !Enum.IsDefined(c.Tipo)))
                return BadRequest(new { mensaje = "El tipo de contacto indicado no es válido." });

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


        #region Equipo
        /// <summary>
        /// Lista el equipo del refugio (fundador y colaboradores). Lo ve cualquiera del refugio
        /// </summary>
        [HttpGet("equipo")]
        public async Task<IActionResult> ListarEquipo()
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            return Ok(await _refugioService.ListarEquipoAsync(refugioId.Value));
        }

        /// <summary>
        /// Dar de alta un colaborador en el refugio, solo el fundador
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("equipo")]
        public async Task<IActionResult> AgregarColaborador([FromBody] ColaboradorRequest request)
        {
            var refugioId = User.ObtenerRefugioId();

            var usuarioId = User.ObtenerUsuarioId();

            if (refugioId == null || usuarioId == null)
                return Forbid();

            var result = await _refugioService.AgregarColaboradorAsync(usuarioId.Value, refugioId.Value, request);

            if(result.Error != null) return BadRequest(new { mensaje = result.Error });

            return Ok(result.Colaborador);
        }


        /// <summary>
        /// Bloquea un colaborador del refugio, solo el fundador
        /// </summary>
        /// <param name="id"></param>
        [HttpPatch("equipo/{id:guid}/bloquear")]
        public async Task<IActionResult> BloquearColaborador(Guid id)
        {
            var refugioId = User.ObtenerRefugioId();

            var usuarioId = User.ObtenerUsuarioId();

            if (refugioId == null || usuarioId == null)
                return Forbid();

            var error = await _refugioService.CambiarEstadoColaboradorAsync(usuarioId.Value, refugioId.Value, id, false);

            if (error != null) return BadRequest(new { mensaje = error });

            return NoContent();

        }

        /// <summary>
        /// Desbloquea un colaborador del refugio, solo el fundador
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("equipo/{id:guid}/desbloquear")]
        public async Task<IActionResult> DesbloquearColaborador(Guid id)
        {
            var refugioId = User.ObtenerRefugioId();

            var usuarioId = User.ObtenerUsuarioId();

            if (refugioId == null || usuarioId == null)
                return Forbid();

            var error = await _refugioService.CambiarEstadoColaboradorAsync(usuarioId.Value, refugioId.Value, id, true);

            if (error != null) return BadRequest(new { mensaje = error });

            return NoContent();

        }
        
        /// <summary>
        /// Blanquea la contraseña de un colaborador del refugio con una temporal, solo el fundador
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch("equipo/{id:guid}/password")]
        public async Task<IActionResult> BlanquearPasswordColaborador(Guid id, [FromBody] BlanquearPasswordRequest request)
        {
            var refugioId = User.ObtenerRefugioId();

            var usuarioId = User.ObtenerUsuarioId();

            if (refugioId == null || usuarioId == null)
                return Forbid();

            var error = await _refugioService.BlanquearPasswordColaboradorAsync(usuarioId.Value, refugioId.Value, id, request.PasswordNueva);

            if (error != null) return BadRequest(new { mensaje = error });

            return NoContent();
        }

        #endregion
    }
}
