using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransitAR.Api.Extensions;
using TransitAR.Api.Services;
using TransitAR.Structures;

namespace TransitAR.Api.Controllers
{

    /// <summary>
    /// Chat interno entre refugio y postulante. La conversacion nace de la postulacion (relacion). Sin rol, lo usan los dos, uso token para identificar
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MensajeController : ControllerBase
    {

        private readonly IMensajeService _mensajeService;

        /// <summary>
        /// Inciializa el servicio de mensajes
        /// </summary>
        /// <param name="mensajeService"></param>
        public MensajeController(IMensajeService mensajeService)
        {
            _mensajeService = mensajeService;
        }


        /// <summary>
        /// Obtengo todos los chat segun quien consulte. FIltros opcionales (una persona peude tener distintas postulaciones)
        /// </summary>
        /// <param name="publicacionId"></param>
        /// <param name="mascotaId"></param>
        /// <returns></returns>
        [HttpGet("chats")]
        public async Task<IActionResult> ListarChats([FromQuery] Guid? publicacionId, [FromQuery] Guid? mascotaId)
        {
            //obtengo el usuario
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId == null)
                return Forbid();

            //null en postulante, sino es un refugio
            var refugioId = User.ObtenerRefugioId();

            return Ok(await _mensajeService.ListarChatsAsync(usuarioId.Value, refugioId, publicacionId, mascotaId));
        }

        /// <summary>
        /// Devuelve la conversacion completa y pone leidos los mensajes del lado que consulto con respecto al que envio
        /// </summary>
        /// <param name="postulacionId"></param>
        /// <returns></returns>
        [HttpGet("{postulacionId:guid}")]
        public async Task<IActionResult> ObtenerChat(Guid postulacionId)
        {
            //obtengo usuario
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId == null)
                return Forbid();

            //obtengo los chats (no tengo que distinguir)
            var chat = await _mensajeService.ObtenerChatAsync(postulacionId, usuarioId.Value, User.ObtenerRefugioId());

            if (chat == null)
                return NotFound(new { mensaje = "No encontramos esa conversacion o no existe." });

            return Ok(chat);
        }


        /// <summary>
        /// Envia un mensaje a una convesacio. Tiene achivos adjuntos ademas del texto
        /// </summary>
        /// <param name="postulacionId"></param>
        /// <param name="request"></param>
        /// <param name="archivos"></param>
        /// <returns></returns>
        [HttpPost("{postulacionId:guid}")]
        public async Task<IActionResult> EnviarMensaje(Guid postulacionId, [FromForm] MensajeRequest request, [FromForm] List<IFormFile>? archivos)
        {
            //busco usuario
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId == null)
                return Forbid();

            var recibidos = archivos ?? new List<IFormFile>();

            //reviso que haya un mensaje o un adjunto
            if (string.IsNullOrWhiteSpace(request.Texto) && recibidos.Count == 0)
                return BadRequest(new { mensaje = "El mensaje tiene que tener texto o un adjunto." });


            //para uso de IFormFile
            var adjuntos = new List<ArchivoAdjunto>();

            //recorro los archivos
            foreach (var archivo in recibidos)
            {
                using var memoria = new MemoryStream();
                await archivo.CopyToAsync(memoria);

                adjuntos.Add(new ArchivoAdjunto
                {
                    NombreArchivo = archivo.FileName,
                    ContentType = archivo.ContentType,
                    Contenido = memoria.ToArray()
                });
            }

            //envio el mensaje con los adjuntos si tiene
            var resultado = await _mensajeService.EnviarAsync(postulacionId, request, adjuntos, usuarioId.Value, User.ObtenerRefugioId());

            if (resultado.Mensaje == null)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(resultado.Mensaje);

        }


        /// <summary>
        /// Descarga un adjunto (tiene que tener accesso)
        /// </summary>
        /// <param name="adjuntoId"></param>
        /// <returns></returns>
        [HttpGet("adjunto/{adjuntoId:guid}")]
        public async Task<IActionResult> ObtenerAdjunto(Guid adjuntoId)
        {
            //obtengo usuario
            var usuarioId = User.ObtenerUsuarioId();
            if (usuarioId == null)
                return Forbid();

            //traigo el adjunto
            var archivo = await _mensajeService.ObtenerAdjuntoAsync(adjuntoId, usuarioId.Value, User.ObtenerRefugioId());

            if (archivo == null)
                return NotFound(new { mensaje = "No encontramos ese adjunto." });

            return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
        }




    }
}
