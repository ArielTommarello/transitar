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
        private readonly IPostulacionService _postulacionService;

         /// <summary>
         /// Inicializa el contexto de Publicaicones
         /// </summary>
         /// <param name="publicacionService"></param>
         /// <param name="postulacionService"></param>
        public PublicacionController(IPublicacionService publicacionService, IPostulacionService postulacionService)
        {
            _publicacionService = publicacionService;
            _postulacionService = postulacionService;
        }

        /// <summary>
        /// Lista las putbliaciones del refugio ya autenticado
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ListarPublicaciones()
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
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
            if (refugioId == null)
                return Forbid();

            var publicacion = await _publicacionService.ObtenerPublicacionAsync(id, refugioId.Value);

            if (publicacion == null)
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
            if (refugioId == null)
                return Forbid();

            var publicacion = await _publicacionService.CrearPublicacionAsync(request, refugioId.Value);

            if (publicacion == null)
                return BadRequest(new { mensaje = "La mascota no existe, no pertenece al refugio, o ya tiene una publicacion abierta o hay un error en los datos de la publicacion" });

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
            if (refugioId == null)
                return Forbid();

            var publicacion = await _publicacionService.ActualizarPublicacionAsync(id, request, refugioId.Value);

            if (publicacion == null)
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
            if (refugioId == null)
                return Forbid();

            var publicacion = await _publicacionService.CambiarEstadoAsync(id, estado, refugioId.Value);

            if (publicacion == null)
                return NotFound();

            return Ok(publicacion);
        }



        //endopoints para las publicaciones desde la vista del refugio

        /// <summary>
        /// Devuelvo la lista de todas las posutlaicones del refugio en base a la publicacion
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}/postulaciones")]
        public async Task<IActionResult> ListarPostulaciones(Guid id)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            return Ok(await _postulacionService.ListarPostulacionesDePublicacionAsync(id, refugioId.Value));
        }

        /// <summary>
        /// Acepta un candidato que se postulao en la publicacion
        /// </summary>
        /// <param name="id"></param>
        /// <param name="postulacionId"></param>
        /// <returns></returns>
        [HttpPatch("{id:guid}/postulaciones/{postulacionId:guid}/aceptar")]
        public async Task<IActionResult> AceptarPostulacion(Guid id, Guid postulacionId)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            var resultado = await _postulacionService.AceptarAsync(id, postulacionId, refugioId.Value);

            if (resultado.Postulacion == null)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(resultado.Postulacion);
        }

        /// <summary>
        /// Rechaza un candidato que se postulao en la publicacion
        /// </summary>
        /// <param name="id"></param>
        /// <param name="postulacionId"></param>
        /// <param name="observacion"></param>
        /// <returns></returns>
        [HttpPatch("{id:guid}/postulaciones/{postulacionId:guid}/rechazar")]
        public async Task<IActionResult> RechazarPostulacion(Guid id, Guid postulacionId,[FromBody] RechazoRequest request)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            var resultado = await _postulacionService.RechazarAsync(id, postulacionId, request?.Observacion, refugioId.Value);

            if (resultado.Postulacion == null)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(resultado.Postulacion);
        }

        /// <summary>
        ///  Lista todas las publciaciones activas de los refugio, uso publico para que lo vea el usuario. Todos los filtros opcionales
        /// </summary>
        /// <param name="tipo"></param>
        /// <param name="especieId"></param>
        /// <param name="tamanio"></param>
        /// <param name="sexo"></param>
        /// <param name="ubicacion"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("activas")]
        public async Task<IActionResult> ListarPublicacionesActivas([FromQuery] TipoPublicacion? tipo,[FromQuery] Guid? especieId,[FromQuery] Tamanio? tamanio,[FromQuery] Sexo? sexo,[FromQuery] string? ubicacion)
        {
            if (tipo != null && !Enum.IsDefined(tipo.Value))
                return BadRequest(new { mensaje = "El tipo de publicacion indicado no es valido." });

            if (tamanio != null && !Enum.IsDefined(tamanio.Value))
                return BadRequest(new { mensaje = "El tamaño indicado no es valido." });

            if (sexo != null && !Enum.IsDefined(sexo.Value))
                return BadRequest(new { mensaje = "El sexo indicado no es valido." });

            return Ok(await _publicacionService.ListarActivasAsync(tipo, especieId, tamanio, sexo, ubicacion));
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

            if (publicacion == null)
                return NotFound();

            return Ok(publicacion);
        }

    }
}
