using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Admin
{
    /// <summary>
    /// El admin agrega, edita y borra especiaes y condiciones de salud 
    /// </summary>
    public class CatalogosModel : PageModel
    {

        private readonly ApiClient _api;

        public CatalogosModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Nombre que se agrega o edita, especie o condicion.
        /// </summary>
        [BindProperty]
        public string? Nombre { get; set; }

        /// <summary>
        /// Desc opcional
        /// </summary>
        [BindProperty]
        public string? Descripcion { get; set; }

        public List<EspecieResponse> Especies { get; set; } = new();

        public List<CondicionResponse> Condiciones { get; set; } = new();

        [TempData]
        public string? Mensaje { get; set; }

        [TempData]
        public string? Error { get; set; }


        public async Task OnGetAsync()
        {
            var especies = await _api.GetAsync<List<EspecieResponse>>("api/Especie");
            var condiciones = await _api.GetAsync<List<CondicionResponse>>("api/Condicion");

            if (!especies.Exito || !condiciones.Exito)
                Error = "No pudimos cargar las listas. Prueba de nuevo.";

            Especies = especies.Datos ?? new();
            Condiciones = condiciones.Datos ?? new();
        }

        #region Especies

        public async Task<IActionResult> OnPostAgregarEspecieAsync()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
                return Volver(null, "Escribe el nombre de la especie.");

            var respuesta = await _api.PostAsync<EspecieResponse>("api/Admin/especies",
                new EspecieRequest { Nombre = Nombre.Trim(), Descripcion = DescripcionLimpia() });

            return Volver(respuesta.Exito ? "Especie agregada." : null, respuesta.Error);
        }

        public async Task<IActionResult> OnPostGuardarEspecieAsync(Guid id)
        {
            if (string.IsNullOrWhiteSpace(Nombre))
                return Volver(null, "El nombre de la especie no puede quedar vacío.");

            var respuesta = await _api.PutAsync<EspecieResponse>($"api/Admin/especies/{id}",
                new EspecieRequest { Nombre = Nombre.Trim(), Descripcion = DescripcionLimpia() });

            return Volver(respuesta.Exito ? "Especie guardada." : null, respuesta.Error);
        }

        public async Task<IActionResult> OnPostBorrarEspecieAsync(Guid id)
        {
            var respuesta = await _api.DeleteAsync<object>($"api/Admin/especies/{id}");

            return Volver(respuesta.Exito ? "Especie borrada." : null, respuesta.Error);
        }

        #endregion



        #region Condiciones

        public async Task<IActionResult> OnPostAgregarCondicionAsync()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
                return Volver(null, "Escribí el nombre de la condición.");

            var respuesta = await _api.PostAsync<CondicionResponse>("api/Admin/condiciones",
                new CondicionRequest { Nombre = Nombre.Trim(), Descripcion = DescripcionLimpia() });

            return Volver(respuesta.Exito ? "Condición agregada." : null, respuesta.Error);
        }

        public async Task<IActionResult> OnPostGuardarCondicionAsync(Guid id)
        {
            if (string.IsNullOrWhiteSpace(Nombre))
                return Volver(null, "El nombre de la condición no puede quedar vacío.");

            var respuesta = await _api.PutAsync<CondicionResponse>($"api/Admin/condiciones/{id}",
                new CondicionRequest { Nombre = Nombre.Trim(), Descripcion = DescripcionLimpia() });

            return Volver(respuesta.Exito ? "Condición guardada." : null, respuesta.Error);
        }

        public async Task<IActionResult> OnPostBorrarCondicionAsync(Guid id)
        {
            var respuesta = await _api.DeleteAsync<object>($"api/Admin/condiciones/{id}");

            return Volver(respuesta.Exito ? "Condición borrada." : null, respuesta.Error);
        }

        #endregion


        //trim para no guardar espacios
        private string? DescripcionLimpia() =>
            string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim();

        //guardar el error y volver atras
        private IActionResult Volver(string? aviso, string? error)
        {
            Mensaje = aviso;
            Error = aviso == null ? error : null;
            return RedirectToPage();
        }


    }
}
