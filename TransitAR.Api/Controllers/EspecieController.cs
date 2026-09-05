using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransitAR.Api.Services;

namespace TransitAR.Api.Controllers
{


    /// <summary>
    /// Especies disponibles, peuden ser las base o las que fue agregando dinamicamente el admin
    /// Se listan en la pagina al momento de creacion o actualizacion de una mascota
    /// </summary>
    
    [Route("api/[controller]")]
    [ApiController]
    public class EspecieController : ControllerBase
    {
        private readonly IEspecieService _especieService;

        /// <summary>
        /// Inicializa el servicio de especies
        /// </summary>
        /// <param name="especieService"></param>
        public EspecieController(IEspecieService especieService)
        {
            _especieService = especieService;
        }

        /// <summary>
        /// Lista las especies disponibles
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> ListarEspecies()
        {
            return Ok(await _especieService.ListarEspeciesAsync());
        }

    }
}
