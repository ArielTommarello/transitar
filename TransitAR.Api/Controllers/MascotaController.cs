using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransitAR.Api.Extensions;
using TransitAR.Api.Services;
using TransitAR.Structures;

namespace TransitAR.Api.Controllers
{


    /// <summary>
    /// ABM mascotas del refugio ya autenticado y correspondiente. 
    /// Las mascotas siempre son sobre ese refugio
    /// </summary>


    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(RolUsuario.Refugio))]
    public class MascotaController : ControllerBase
    {



        private readonly IMascotaService _mascotaService;

        /// <summary>
        /// Inicializa el servicio de mascotas
        /// </summary>
        /// <param name="mascotaService"></param>
        public MascotaController(IMascotaService mascotaService)
        {
            _mascotaService = mascotaService;
        }

        /// <summary>
        /// Lista todas las mascotas del refugio correspondiente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListarMascotas()
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId is null)
                return Forbid();

            var mascotas = await _mascotaService.ListarMascotasAsync(refugioId.Value);
            return Ok(mascotas);
        }

        /// <summary>
        /// Devuelve una mascota del refugio correspondiente, busqueda por id
        /// </summary>
        /// <param name="id">id mascota</param>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObtenerMascota(Guid id)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId is null)
                return Forbid();

            var mascota = await _mascotaService.ObtenerMascotaAsync(id, refugioId.Value);

            if (mascota is null)
                return NotFound();

            return Ok(mascota);
        }

        /// <summary>
        /// Carga una mascota nueva en el refugio 
        /// </summary>
        /// <param name="request">datos de la mascota</param>
        [HttpPost]
        public async Task<IActionResult> CrearMascota([FromBody] MascotaRequest request)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId is null)
                return Forbid();

            var mascota = await _mascotaService.CrearMascotaAsync(request, refugioId.Value);

            if (mascota is null)
                return BadRequest(new { mensaje = "La especie o la condicion indicada no existe." });

            return Ok(mascota);
        }

        /// <summary>
        /// Actualiza una mascota del refugio au
        /// </summary>
        /// <param name="id">id mascota</param>
        /// <param name="request">datos nuevos</param>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> ActualizarMascota(Guid id, [FromBody] MascotaRequest request)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId is null)
                return Forbid();

            var mascota = await _mascotaService.ActualizarMascotaAsync(id, request, refugioId.Value);

            if (mascota is null)
                return NotFound();

            return Ok(mascota);
        }

        /// <summary>
        /// Elimina una mascota del refugio 
        /// </summary>
        /// <param name="id">id mascota</param>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> EliminarMascota(Guid id)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId is null)
                return Forbid();

            if (!await _mascotaService.EliminarMascotaAsync(id, refugioId.Value))
                return NotFound();

            return NoContent();
        }

    }
}
