using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Cuenta
{

    /// <summary>
    /// Registro de un Postulante que uqiere adoptar o transitar (rol usuario)
    /// </summary>
    public class RegistroPostulanteModel : PageModel
    {
        private readonly ApiClient _api;

        public RegistroPostulanteModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Datos que completea el posutalnte
        /// </summary>
        [BindProperty]
        public RegistroPostulanteRequest Datos { get; set; } = new();

        /// <summary>
        /// Mensaje de error de la API
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Mensaje luego del resgistro
        /// </summary>
        [TempData]
        public string? Mensaje { get; set; }


        //viene con la pagina
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            //respeusta en caso de exito
            var respuesta = await _api.PostAsync<object>("api/Auth/registro/postulante", Datos);

            if (!respuesta.Exito)
            {
                Error = respuesta.Error;
                return Page();
            }

            Mensaje = "Cuenta creada. Ya podés iniciar sesión.";
            return RedirectToPage("/Cuenta/Login");
        }
    }
}
