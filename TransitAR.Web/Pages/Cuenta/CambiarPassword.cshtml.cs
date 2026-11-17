using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Web.Services;
using TransitAR.Structures;
using System.ComponentModel.DataAnnotations;


namespace TransitAR.Web.Pages.Cuenta
{
    [Authorize]
    public class CambiarPasswordModel : PageModel
    {

        private readonly ApiClient _api;

        public CambiarPasswordModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Datos que completa la persona
        /// </summary>
        [BindProperty]
        public CambiarPasswordFormulario Datos { get; set; } = new();

        /// <summary>
        /// Mensaje de error de la API
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Mensaje despues de cambiar la contraseña
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

            var respuesta = await _api.PutAsync<object>("api/Auth/password", new CambiarPasswordRequest { PasswordActual = Datos.PasswordActual, PasswordNueva= Datos.PasswordNueva});

            if (!respuesta.Exito)
            {
                Error = respuesta.Error;
                return Page();
            }

            Mensaje = "Contraseña actualizada.";

            return RedirectToPage();
        }
    }

    /// <summary>
    /// Formulario de la web. Confirmar se valida solo aca, no va a la api
    /// </summary>
    public class CambiarPasswordFormulario
    {
        [Required]
        [Display(Name = "Contraseña actual")]
        public string PasswordActual { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [MaxLength(100)]
        [Display(Name = "Contraseña nueva")]
        public string PasswordNueva { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(PasswordNueva), ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña nueva")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
