using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransitAR.Api.Services;
using TransitAR.Structures;

namespace TransitAR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(RolUsuario.Admin))]
    public class AdminController : ControllerBase
    {

        private readonly IEspecieService _especieService;
        private readonly ICondicionService _condicionService;

        /// <summary>
        /// Inicializa los servicios para que los administre el admin
        /// </summary>
        /// <param name="especieService"></param>
        /// <param name="condicionService"></param>
        public AdminController(IEspecieService especieService, ICondicionService condicionService)

        {
            _especieService = especieService;
            _condicionService = condicionService;
        }

        #region Especies

        /// <summary>
        /// Crea una especie
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("especies")]
        public async Task<IActionResult> CrearEspecie([FromBody] EspecieRequest request)
        {
            var resultado = await _especieService.CrearEspecieAsync(request);

            if (resultado.Especie == null)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(resultado.Especie);
        }

        /// <summary>
        /// Edita una especie
        /// </summary>
        [HttpPut("especies/{id:guid}")]
        public async Task<IActionResult> ActualizarEspecie(Guid id, [FromBody] EspecieRequest request)
        {
            var resultado = await _especieService.ActualizarEspecieAsync(id, request);

            if (resultado.Especie == null)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(resultado.Especie);
        }

        /// <summary>
        /// Borra una especie si ninguna mascota la usa
        /// </summary>
        [HttpDelete("especies/{id:guid}")]
        public async Task<IActionResult> EliminarEspecie(Guid id)
        {
            var error = await _especieService.EliminarEspecieAsync(id);

            if (error != null)
                return BadRequest(new { mensaje = error });

            return NoContent();
        }

        #endregion

        #region Condiciones
        /// <summary>
        /// Crea una condicion
        /// </summary>
        [HttpPost("condiciones")]
        public async Task<IActionResult> CrearCondicion([FromBody] CondicionRequest request)
        {
            var resultado = await _condicionService.CrearCondicionAsync(request);

            if (resultado.Condicion == null)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(resultado.Condicion);
        }

        /// <summary>
        /// Edita una condicion
        /// </summary>
        [HttpPut("condiciones/{id:guid}")]
        public async Task<IActionResult> ActualizarCondicion(Guid id, [FromBody] CondicionRequest request)
        {
            var resultado = await _condicionService.ActualizarCondicionAsync(id, request);

            if (resultado.Condicion == null)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(resultado.Condicion);
        }

        /// <summary>
        /// Borra una condicion si ninguna mascota la usa
        /// </summary>
        [HttpDelete("condiciones/{id:guid}")]
        public async Task<IActionResult> EliminarCondicion(Guid id)
        {
            var error = await _condicionService.EliminarCondicionAsync(id);

            if (error != null)
                return BadRequest(new { mensaje = error });

            return NoContent();
        }
        
        #endregion
    }
}
