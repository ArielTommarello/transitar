using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Cuenta
{
    /// <summary>
    /// Registro de un refugio, se crea el refugio y la cuenta de user fundador. (reol refugio)
    /// </summary>
    public class RegistroRefugioModel : PageModel
    {
        private readonly ApiClient _api;

        public RegistroRefugioModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Datos del refugio
        /// </summary>
        [BindProperty]
        public RegistroRefugioRequest Datos { get; set; } = new();

        /// <summary>
        /// Mensaje de error de la API
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Mensaje luego del resgistro
        /// </summary>
        [TempData]
        public string? Mensaje { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var respuesta = await _api.PostAsync<object>("api/Auth/registro/refugio", Datos);

            if (!respuesta.Exito)
            {
                Error = respuesta.Error;
                return Page();
            }

            Mensaje = "Refugio registrado. Ya podés iniciar sesión.";
            return RedirectToPage("/Cuenta/Login");
        }
    }
}
