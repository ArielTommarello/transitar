using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransitAR.Api.Extensions;
using TransitAR.Api.Services;
using TransitAR.Structures;
using TransitAR.Structures.Requests;

namespace TransitAR.Api.Controllers
{
    /// <summary>
    /// Controller para el seguimiento de las tenencias, aca esta el lado del refugio que e sle que mas hace, agenda, crea reporgrama y califica segun estado. Tambien hace ls observaciones
    /// En postulacionController puse la de las personas
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(RolUsuario.Refugio))]
    public class SeguimientoController : ControllerBase
    {
        private readonly ISeguimientoService _seguimientoService;

        /// <summary>
        /// Inicializa el servicio de seguimiento
        /// </summary>
        /// <param name="seguimientoService"></param>
        public SeguimientoController(ISeguimientoService seguimientoService)
        {
            _seguimientoService = seguimientoService;
        }

        /// <summary>
        /// Lista todos los seguimientos de las tenencias de las masctoas del refugio
        /// </summary>
        /// <param name="estado"></param>
        /// <param name="vencidos"></param>
        /// <param name="mascotaId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] EstadoSeguimiento? estado, [FromQuery] bool vencidos = false, [FromQuery] Guid? mascotaId = null)
        {
            //busco los actores que estan en la tenencia
            var usuarioId = User.ObtenerUsuarioId();
            var refugioId = User.ObtenerRefugioId();
            if (usuarioId == null || refugioId == null)
                return Forbid();

            if (estado != null && !Enum.IsDefined(estado.Value))
                return BadRequest(new { mensaje = "El estado indicado no es valido o correcto." });

            return Ok(await _seguimientoService.ListarAsync(usuarioId.Value, refugioId, estado, vencidos, mascotaId));
        }


        /// <summary>
        /// Crea un control sobre una tenencia, la agenda y la pone como pendiente.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] SeguimientoRequest request)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            var error = await _seguimientoService.CrearAsync(request, refugioId.Value);

            if (error != null)
                return BadRequest(new { mensaje = error });

            return NoContent();
        }



        /// <summary>
        /// Cambia el estado (realizado, cancelado, reprogramado) o reprograma en fecha un control. EL viejo lo guardo para que quede de historial con una observacion
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] SeguimientoRequest request)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            var error = await _seguimientoService.CambiarEstadoAsync(id, request, refugioId.Value);

            if (error != null)
                return BadRequest(new { mensaje = error });

            return NoContent();
        }

    }
}
