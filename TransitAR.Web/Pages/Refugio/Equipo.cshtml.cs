using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Refugio
{
    public class EquipoModel : PageModel
    {

        private readonly ApiClient _api;

        public EquipoModel(ApiClient api)
        {
            _api = api;
        }


        /// <summary>
        /// Lista del equipo que pertence al refugio (miembros)
        /// </summary>
        public List<MiembroEquipoResponse> Equipo { get; set; } = new();

        /// <summary>
        /// Si es fundador, para mostrar los formularios
        /// </summary>
        public bool EsFundador { get; set; }

        /// <summary>
        /// Nuevo colaborador (formulario)
        /// </summary>
        public ColaboradorRequest Nuevo { get; set; } = new();

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


        public async Task OnGet()
        {
            await CargarAsync();            
        }


        private async Task CargarAsync()
        {
            var respuesta = await _api.GetAsync<List<MiembroEquipoResponse>>("api/Refugio/equipo");

            if (!respuesta.Exito)
                Error = respuesta.Error;

            Equipo = respuesta.Datos ?? new();

            //el fundador se reconoce buscandose a si mismo en la lista. Solo para esconder botones
            var miId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            EsFundador = Equipo.Any(m => m.Id.ToString() == miId && m.RolRefugio == RolRefugio.Fundador);
        }


        /// <summary>
        /// Agrego el colaborador (solo fundador)
        /// </summary>
        /// <param name="nuevo"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAgregarAsync([Bind(Prefix = "Nuevo")] ColaboradorRequest nuevo)
        {
            if (!ModelState.IsValid)
            {
                Nuevo = nuevo;
                await CargarAsync();
                return Page();               
            }

            var respuesta = await _api.PostAsync<MiembroEquipoResponse>("api/Refugio/equipo", nuevo);

            return Volver(respuesta.Exito ? "Colaborador agregado. Pasale su email y la contraseña inicial." : null, respuesta.Error);

        }


        public async Task<IActionResult> OnPostBloquearAsync(Guid id)
        {
            var respuesta = await _api.PatchAsync<object>($"api/Refugio/equipo/{id}/bloquear");
            return Volver(respuesta.Exito ? "Colaborador bloqueado." : null, respuesta.Error);

        }

        public async Task<IActionResult> OnPostDesbloquearAsync(Guid id)
        {
            var respuesta = await _api.PatchAsync<object>($"api/Refugio/equipo/{id}/desbloquear");
            return Volver(respuesta.Exito ? "Colaborador desbloqueado." : null, respuesta.Error);

        }


        public async Task<IActionResult> OnPostBlanquearAsync(Guid id, string passwordNueva)
        {
            var respuesta = await _api.PatchAsync<object>($"api/Refugio/equipo/{id}/password",
                new BlanquearPasswordRequest { PasswordNueva = passwordNueva });
            return Volver(respuesta.Exito ? "Contraseña blanqueada. Comunicásela al colaborador." : null, respuesta.Error);
        }


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
            return RedirectToPage();
        }
    }
}
