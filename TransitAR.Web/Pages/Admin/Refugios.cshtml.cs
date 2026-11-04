using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Admin
{

    /// <summary>
    /// Refugios que ve el admin todos, puede bloquearlos y desbloquearlos, si se bloquea se bajan sus publicaciones
    /// </summary>
    public class RefugiosModel : PageModel
    {

        private readonly ApiClient _api;

        public RefugiosModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Filtro por estado (null = todos, true = activos, false = bloqueados)
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public bool? Activo { get; set; }

        /// <summary>
        /// Todos los refugios para cada pestaña
        /// </summary>
        public List<RefugioAdminResponse> Todos { get; set; } = new();

        /// <summary>
        /// Los que pasan el filtro
        /// </summary>
        public List<RefugioAdminResponse> Refugios =>
            Todos.Where(r => Activo == null || r.Activo == Activo).ToList();


        /// <summary>
        /// Mensaje
        /// </summary>
        [TempData]
        public string? Mensaje { get; set; }
        
        /// <summary>
        /// Errror
        /// </summary>
        [TempData]
        public string? Error { get; set; }



        public async Task OnGetAsync()
        {
            var respuesta = await _api.GetAsync<List<RefugioAdminResponse>>("api/Admin/refugios");

            if (!respuesta.Exito)
                Error = respuesta.Error;

            Todos = respuesta.Datos ?? new();
        }

        public async Task<IActionResult> OnPostBloquearAsync(Guid id)
        {
            var respuesta = await _api.PatchAsync<object>($"api/Admin/refugios/{id}/bloquear");
            return Volver(respuesta.Exito ? "Refugio bloqueado." : null, respuesta.Error);
        }


        public async Task<IActionResult> OnPostDesbloquearAsync(Guid id)
        {
            var respuesta = await _api.PatchAsync<object>($"api/Admin/refugios/{id}/desbloquear");
            return Volver(respuesta.Exito ? "Refugio desbloqueado." : null, respuesta.Error);
        }

        /// <summary>
        /// Cantidad para usar en las pestañas
        /// </summary>
        /// <param name="activo"></param>
        /// <returns></returns>
        public int Cantidad(bool? activo) => Todos.Count(r => activo == null || r.Activo == activo);

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
            return RedirectToPage(new { Activo });
        }
    }
}
