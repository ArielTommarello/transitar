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
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Inicializa el servicio de mascotas
        /// </summary>
        /// <param name="mascotaService"></param>
        public MascotaController(IMascotaService mascotaService, IConfiguration configuration)
        {
            _mascotaService = mascotaService;
            _configuration = configuration; 
        }

        /// <summary>
        /// Lista todas las mascotas del refugio correspondiente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListarMascotas()
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
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
            if (refugioId == null)
                return Forbid();

            var mascota = await _mascotaService.ObtenerMascotaAsync(id, refugioId.Value);

            if (mascota == null)
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
            if (refugioId == null)
                return Forbid();

            var mascota = await _mascotaService.CrearMascotaAsync(request, refugioId.Value);

            if (mascota == null)
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
            if (refugioId == null)
                return Forbid();

            var mascota = await _mascotaService.ActualizarMascotaAsync(id, request, refugioId.Value);

            if (mascota == null)
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
            if (refugioId == null)
                return Forbid();

            if (!await _mascotaService.EliminarMascotaAsync(id, refugioId.Value))
                return NotFound();

            return NoContent();
        }



        //FOTOS ADJUNTAS MASCOTA

        /// <summary>
        /// Sube una ovarias fotos  a una mascota del refugio
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fotos"></param>
        /// <returns></returns>
        [HttpPost("{id:guid}/fotos")]
        public async Task<IActionResult> AgregarFotos(Guid id, List<IFormFile> fotos)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            if (fotos == null || fotos.Count == 0)
                return BadRequest(new { mensaje = "No se recibio ninguna foto." });

            //valida igual que la de perfil (reglas)
            var maxMB = _configuration.GetValue<int?>("Reglas:FotoPerfilMaxMB") ?? 5;
            foreach (var foto in fotos)
            {
                var error = FotoPerfilArchivo.Validar(foto, maxMB);
                if (error != null)
                    return BadRequest(new { mensaje = $"{foto.FileName}: {error}" });
            }

            var archivos = new List<ArchivoAdjunto>();
            foreach (var foto in fotos)
                archivos.Add(await FotoPerfilArchivo.LeerAsync(foto));

            //valida igual que la de perfil (reglas)
            var maximo = _configuration.GetValue<int?>("Reglas:FotosMascotaMax") ?? 6;
            var errorServicio = await _mascotaService.AgregarFotosAsync(id, refugioId.Value, archivos, maximo);

            if (errorServicio != null)
                return BadRequest(new { mensaje = errorServicio });

            return NoContent();
        }

        /// <summary>
        /// Borra una foto de la masctoa del refugio
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fotoId"></param>
        /// <returns></returns>
        [HttpDelete("{id:guid}/fotos/{fotoId:guid}")]
        public async Task<IActionResult> BorrarFoto(Guid id, Guid fotoId)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            if (!await _mascotaService.BorrarFotoAsync(id, fotoId, refugioId.Value))
                return NotFound(new { mensaje = "No encontramos la foto en tus mascotas." });

            return NoContent();
        }

        /// <summary>
        /// HAce principal una foto seleccionada, asi se meustra en la tarjeta de la mascota
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fotoId"></param>
        /// <returns></returns>
        [HttpPatch("{id:guid}/fotos/{fotoId:guid}/principal")]
        public async Task<IActionResult> HacerPrincipal(Guid id, Guid fotoId)
        {
            var refugioId = User.ObtenerRefugioId();
            if (refugioId == null)
                return Forbid();

            if (!await _mascotaService.HacerPrincipalAsync(id, fotoId, refugioId.Value))
                return NotFound(new { mensaje = "No encontramos la foto en tus mascotas." });

            return NoContent();
        }


        /// <summary>
        /// Muestra la foto de una mascota (publico)
        /// </summary>
        /// <param name="fotoId"></param>
        /// <returns></returns>
        [HttpGet("fotos/{fotoId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObtenerFoto(Guid fotoId)
        {
            var foto = await _mascotaService.ObtenerFotoAsync(fotoId);

            if (foto == null)
                return NotFound();

            return File(foto.Contenido, foto.ContentType);
        }

    }
}
