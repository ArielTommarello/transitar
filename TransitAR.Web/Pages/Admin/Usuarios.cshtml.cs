using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Admin
{

    /// <summary>
    /// Usuarios que ve el admin, y puede bloquear
    /// </summary>
    public class UsuariosModel : PageModel
    {

        private readonly ApiClient _api;

        public UsuariosModel(ApiClient api)
        {
            _api = api;
        }


        /// <summary>
        /// Filtro por rol (null = todos)
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public RolUsuario? Rol { get; set; }

        /// <summary>
        /// Filtro por estado (null = todos, true = activos, false = bloqueados)
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public bool? Activo { get; set; }

        /// <summary>
        /// Todos los usuarios  para contar cada pestaña
        /// </summary>
        public List<UsuarioAdminResponse> Todos { get; set; } = new();

        /// <summary>
        /// Los que pasan los dos filtros
        /// </summary>
        public List<UsuarioAdminResponse> Usuarios =>
            Todos.Where(u => Habilitado(u, Rol, Activo)).ToList();


        /// <summary>
        /// Mensaje
        /// </summary>
        [TempData]
        public string? Mensaje { get; set; }


        /// <summary>
        /// Error
        /// </summary>
        [TempData]
        public string? Error { get; set; }

        public async Task OnGetAsync()
        {
            var respuesta = await _api.GetAsync<List<UsuarioAdminResponse>>("api/Admin/usuarios");

            if (!respuesta.Exito)
                Error = respuesta.Error;

            Todos = respuesta.Datos ?? new();
        }

        public async Task<IActionResult> OnPostBloquearAsync(Guid id)
        {
            var respuesta = await _api.PatchAsync<object>($"api/Admin/usuarios/{id}/bloquear");
            return Volver(respuesta.Exito ? "Usuario bloqueado." : null, respuesta.Error);
        }

        public async Task<IActionResult> OnPostDesbloquearAsync(Guid id)
        {
            var respuesta = await _api.PatchAsync<object>($"api/Admin/usuarios/{id}/desbloquear");
            return Volver(respuesta.Exito ? "Usuario desbloqueado." : null, respuesta.Error);
        }

        /// <summary>
        /// Cantidad para usar en las pestañas
        /// </summary>
        /// <param name="rol"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        public int Cantidad(RolUsuario? rol, bool? activo) => Todos.Count(u => Habilitado(u, rol, activo));

        /// <summary>
        /// si el usuario esta habilitado o pasa los filtros 
        /// </summary>
        /// <param name="u"></param>
        /// <param name="rol"></param>
        /// <param name="activo"></param>
        /// <returns></returns>
        private static bool Habilitado(UsuarioAdminResponse u, RolUsuario? rol, bool? activo) =>
            (rol == null || u.Rol == rol) && (activo == null || u.Activo == activo);


        /// <summary>
        /// Aviso y o error y volver para atras con todo guardado
        /// </summary>
        /// <param name="aviso"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        private IActionResult Volver(string? aviso, string? error)
        {
            Mensaje = aviso;
            Error = aviso == null ? error : null;
            return RedirectToPage(new { Rol, Activo });
        }
    }
}
