using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransitAR.Api.Services;

namespace TransitAR.Api.Controllers
{

    /// <summary>
    /// Condiciones disponibles, peuden ser las base o las que fue agregando dinamicamente el admin
    /// Se listan en la pagina al momento de creacion o actualizacion de una mascota
    /// </summary>


    [Route("api/[controller]")]
    [ApiController]
    public class CondicionController : ControllerBase
    {

        private readonly ICondicionService _condicionService;

        /// <summary>
        /// Inicializa el servicio de especies
        /// </summary>
        /// <param name="especieService"></param>
        public CondicionController(ICondicionService condicionService)
        {
            _condicionService = condicionService;
        }

        /// <summary>
        /// Lista las condiciones disponibles
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ListarCondiciones()
        {
            return Ok(await _condicionService.ListarCondicionesAsync());
        }

    }
}
