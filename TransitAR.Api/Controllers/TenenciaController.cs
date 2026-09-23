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
    /// Tenencia de los animales del refuigo, entrega confirmada a quien cuando y donde se lo entrego y como fue su temrinacion (transitos)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(RolUsuario.Refugio))]
    public class TenenciaController : ControllerBase
    {
        private readonly ITenenciaService _tenenciaService;


        /// <summary>
        /// Inicializa serviico de tenencias
        /// </summary>
        /// <param name="tenenciaService"></param>
        public TenenciaController(ITenenciaService tenenciaService)
        {
            _tenenciaService = tenenciaService;
        }


        /// <summary>
        /// Devuelve una tenencia de un animal del refugio por id
        /// </summary>
        /// <param name="id"></param>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObtenerTenencia(Guid id)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            var tenencia = await _tenenciaService.ObtenerTenenciaAsync(id, refugioId.Value);

            if (tenencia == null)
                return NotFound();

            return Ok(tenencia);
        }




        /// <summary>
        /// Confirma la entrega de una postulacion aceptada crea la tenencia, cierra la publicacion, libera a los que estaban en espera y cambia el estado de la mascota
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        public async Task<IActionResult> ConfirmarEntrega([FromBody] TenenciaRequest request)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            var resultado = await _tenenciaService.ConfirmarEntregaAsync(request, refugioId.Value);

            if (resultado.Tenencia == null)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(resultado.Tenencia);
        }


        //agenda para refugios (filtro por vencimiento en tenencias) y listo las tenencias

        /// <summary>
        /// Lista las tenencia de los animales del refugio con filtros opcionales, se usa para tener  los vencimientos cercanos de las tenencias (transitos)
        /// </summary>
        /// <param name="modalidad"></param>
        /// <param name="enCurso"></param>
        /// <param name="vencidas"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ListarTenencias([FromQuery] TipoPublicacion? modalidad, [FromQuery] bool? enCurso, [FromQuery] bool vencidas = false)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            //diferencio de adopcion que no teiene vencimiento y no tendriamos que ver en la agenda 
            if (modalidad != null && !Enum.IsDefined(modalidad.Value))
                return BadRequest(new { mensaje = "La modalidad indicada no es valida." });

            return Ok(await _tenenciaService.ListarTenenciasAsync(refugioId.Value, modalidad, enCurso, vencidas));
        }


        //uso en devoluciones

        /// <summary>
        /// Cierra una tenencia cuando el animal vuelve al refugio. La mascota queda disponible para publicarse de nuevo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        [HttpPatch("{id:guid}/devolver")]
        public async Task<IActionResult> Devolver(Guid id, [FromBody] DevolucionRequest request)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            var resultado = await _tenenciaService.DevolverAsync(id, request, refugioId.Value);

            if (resultado.Tenencia == null)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(resultado.Tenencia);
        }

        /// <summary>
        /// Convierte un transito en curso en una adopcion definitiva , no creo una tenencia nueva porque el animal nunca se movio de esa casa
        /// </summary>
        /// <param name="id"></param>
        [HttpPatch("{id:guid}/convertir")]
        public async Task<IActionResult> ConvertirAAdopcion(Guid id)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            var resultado = await _tenenciaService.ConvertirAAdopcionAsync(id, refugioId.Value);

            if (resultado.Tenencia == null)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(resultado.Tenencia);
        }


        //USO PARA HISTORIAL DE TENENCIAS (USO PARA REFUGIOS)
        /// <summary>
        /// Devuelve el historial de tenencias de un postulante para que el refugio puedea evalaur el cancidato. Tiene antecedentes con otros refugios
        /// </summary>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
        [HttpGet("postulante/{usuarioId:guid}")]
        public async Task<IActionResult> ObtenerHistorialPostulante(Guid usuarioId)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            var historial = await _tenenciaService.ObtenerHistorialPostulanteAsync(usuarioId, refugioId.Value);

            if (historial == null)
                return NotFound(new { mensaje = "No encontramos postulaciones de esa persona en tus publicaciones." });

            return Ok(historial);
        }
    }
}
