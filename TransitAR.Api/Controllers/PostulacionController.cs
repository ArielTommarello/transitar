using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransitAR.Api.Extensions;
using TransitAR.Api.Services;
using TransitAR.Structures;

namespace TransitAR.Api.Controllers
{
    /// <summary>
    /// Postulaciones del usuario el refugio ve las que recibe desde sus propias publicaciones
    /// </summary

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(RolUsuario.Usuario))]
    public class PostulacionController : ControllerBase
    {

        private readonly IPostulacionService _postulacionService;

        /// <summary>
        /// Inicializa el servicio de postulaciones
        /// </summary>
        /// <param name="postulacionService"></param>
        public PostulacionController(IPostulacionService postulacionService)
        {
            _postulacionService = postulacionService;
        }

        /// <summary>
        /// Lista las postulaciones del usuario 
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListarMisPostulaciones()
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId is null)
                return Forbid();

            return Ok(await _postulacionService.ListarMisPostulacionesAsync(usuarioId.Value));
        }

        /// <summary>
        /// Devuelve una postulacion del usuario especifica
        /// </summary>
        /// <param name="id"></param>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObtenerPostulacion(Guid id)
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId is null)
                return Forbid();

            var postulacion = await _postulacionService.ObtenerPostulacionAsync(id, usuarioId.Value);

            if (postulacion is null)
                return NotFound();

            return Ok(postulacion);
        }

        /// <summary>
        /// Postula al usuario a una publicacion activa
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        public async Task<IActionResult> Postularse([FromBody] PostulacionRequest request)
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId is null)
                return Forbid();

            //puede estar vacia, pero no puedo tener horarios random o no validos
            if (request.DisponibilidadHorario.HasValue
             && !Enum.IsDefined(request.DisponibilidadHorario.Value))
                return BadRequest(new { mensaje = "La disponibilidad horaria indicada no es valida o no es correcta." });

            var resultado = await _postulacionService.PostularseAsync(request, usuarioId.Value);

            if (resultado.Postulacion is null)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(resultado.Postulacion);
        }

        /// <summary>
        /// Retira una postulacion propia que todavia no fue resuelta o aceptada
        /// </summary>
        /// <param name="id"></param>
        [HttpPatch("{id:guid}/retirar")]
        public async Task<IActionResult> RetirarPostulacion(Guid id)
        {
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId is null)
                return Forbid();

            var resultado = await _postulacionService.RetirarPostulacionAsync(id, usuarioId.Value);

            if (resultado.Postulacion is null)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(resultado.Postulacion);
        }

    }
}
