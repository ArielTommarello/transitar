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
        private readonly IAdminService _adminService;

        /// <summary>
        /// Inicializa los servicios para que los administre el admin
        /// </summary>
        /// <param name="especieService"></param>
        /// <param name="condicionService"></param>
        public AdminController(IEspecieService especieService, ICondicionService condicionService, IAdminService adminService)

        {
            _especieService = especieService;
            _condicionService = condicionService;
            _adminService = adminService;   
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

        #region Usuarios
        /// <summary>
        /// Lista los usuarios del sistema. Cada filtro es opcional para que lo vea el admin
        /// </summary>
        /// <param name="rol"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        [HttpGet("usuarios")]
        public async Task<IActionResult> ListarUsuarios([FromQuery] RolUsuario? rol, [FromQuery] bool? activo)
        {
            if (rol != null && !Enum.IsDefined(rol.Value))
                return BadRequest(new { mensaje = "El rol indicado no es valido." });

            return Ok(await _adminService.ListarUsuariosAsync(rol, activo));
        }

        /// <summary>
        /// Pone en  no activo a un usuario (Solo el admin)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("usuarios/{id:guid}/bloquear")]
        public async Task<IActionResult> BloquearUsuario(Guid id)
        {
            var error = await _adminService.CambiarEstadoUsuarioAsync(id, false);

            if (error != null)
                return BadRequest(new { mensaje = error });

            return NoContent();
        }

        /// <summary>
        /// Pone en  activo a un usuario (Solo el admin)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("usuarios/{id:guid}/desbloquear")]
        public async Task<IActionResult> DesbloquearUsuario(Guid id)
        {
            var error = await _adminService.CambiarEstadoUsuarioAsync(id, true);

            if (error != null)
                return BadRequest(new { mensaje = error });

            return NoContent();
        }

        #endregion

        #region Refugios

        /// <summary>
        /// Lista todos los refugios del sistema
        /// </summary>
        /// <param name="activo"></param>
        /// <returns></returns>
        [HttpGet("refugios")]
        public async Task<IActionResult> ListarRefugios([FromQuery] bool? activo)
        {
            return Ok(await _adminService.ListarRefugiosAsync(activo));
        }


        /// <summary>
        /// Pone en no activo a un refugio (Solo el admin). Bloquea al refugio y a sus usuarios para que no inicien sesion, y sus publicaciones
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("refugios/{id:guid}/bloquear")]
        public async Task<IActionResult> BloquearRefugio(Guid id)
        {
            var error = await _adminService.CambiarEstadoRefugioAsync(id, false);

            if (error != null)
                return BadRequest(new { mensaje = error });

            return NoContent();
        }

        /// <summary>
        /// Pone en activo a un refugio (Solo el admin). Bloquea al refugio y a sus usuarios para que no inicien sesion, y sus publicaciones
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("refugios/{id:guid}/desbloquear")]
        public async Task<IActionResult> DesbloquearRefugio(Guid id)
        {
            var error = await _adminService.CambiarEstadoRefugioAsync(id, true);

            if (error != null)
                return BadRequest(new { mensaje = error });

            return NoContent();
        }
        #endregion
    }
}
