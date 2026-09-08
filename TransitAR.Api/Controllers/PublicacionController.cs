using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransitAR.Api.Extensions;
using TransitAR.Api.Services;
using TransitAR.Structures;

namespace TransitAR.Api.Controllers
{

    /// <summary>
    /// Publicaciones del refugio autenticado, y el listado publico que ve cualquier persona sin necesidad de cuenta (anonimo)
    /// </summary>

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(RolUsuario.Refugio))]
    public class PublicacionController : ControllerBase
    {

        private readonly IPublicacionService _publicacionService;

        /// <summary>
        /// Inicializa el servicio de publicaicones
        /// </summary>
        /// <param name="publicacionService"></param>
        public PublicacionController(IPublicacionService publicacionService)
        {
            _publicacionService = publicacionService;
        }

        /// <summary>
        /// Lista las putbliaciones del refugio ya autenticado
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ListarPublicaciones()
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId is null)
                return Forbid();

            return Ok(await _publicacionService.ListarPublicacionesAsync(refugioId.Value));
        }

        /// <summary>
        /// Devuelve una publicacion especifica del refugio , busqueda por id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObtenerPublicacion(Guid id)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId is null)
                return Forbid();

            var publicacion = await _publicacionService.ObtenerPublicacionAsync(id, refugioId.Value);

            if (publicacion is null)
                return NotFound();

            return Ok(publicacion);
        }

        /// <summary>
        /// Crea una publicacion sobre una mascota del refugio
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CrearPublicacion([FromBody] PublicacionRequest request)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId is null)
                return Forbid();

            var publicacion = await _publicacionService.CrearPublicacionAsync(request, refugioId.Value);

            if (publicacion is null)
                return BadRequest(new { mensaje = "La mascota no existe, no pertenece al refugio, o ya tiene una publicacion abierta." });

            return Ok(publicacion);
        }

        /// <summary>
        /// Actualiza una publicaicon sobre una mascota del refugio
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> ActualizarPublicacion(Guid id, [FromBody] PublicacionRequest request)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId is null)
                return Forbid();

            var publicacion = await _publicacionService.ActualizarPublicacionAsync(id, request, refugioId.Value);

            if (publicacion is null)
                return NotFound();

            return Ok(publicacion);
        }


        /// <summary>
        /// Permite cambiar el estado de una publicacion (Activa o pausada , cerrada)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        [HttpPatch("{id:guid}/estado/{estado}")]
        public async Task<IActionResult> CambiarEstado(Guid id, EstadoPublicacion estado)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId is null)
                return Forbid();

            var publicacion = await _publicacionService.CambiarEstadoAsync(id, estado, refugioId.Value);

            if (publicacion is null)
                return NotFound();

            return Ok(publicacion);
        }


        /// <summary>
        /// Lista todas las publicaciones activas de TODOS los refugios, es publico y pueden verlo gente sin registrar
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("activas")]
        public async Task<IActionResult> ListarPublicacionesActivas()
        {
            return Ok(await _publicacionService.ListarActivasAsync());
        }


        /// <summary>
        /// Devuelve una publicaicon activa, es publico y pueden verlo gente sin registrar
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("activas/{id:guid}")]
        public async Task<IActionResult> ObtenerPublicacionActiva(Guid id)
        {
            var publicacion = await _publicacionService.ObtenerActivaAsync(id);

            if (publicacion is null)
                return NotFound();

            return Ok(publicacion);
        }

    }
}
