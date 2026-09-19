using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransitAR.Api.Extensions;
using TransitAR.Api.Services;
using TransitAR.Structures;

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
        /// Lista las tenencias de los animales del refugio 
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListarTenencias()
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            return Ok(await _tenenciaService.ListarTenenciasAsync(refugioId.Value));
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


    }
}
