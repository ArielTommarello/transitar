using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures.Requests;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Refugio
{

    /// <summary>
    /// Detalle de una tenencia para el refugio, tine los datos controles y si hay devolcuion (se puede pasar de transito a adopcion)
    /// </summary>
    public class TenenciaModel : PageModel
    {
        private readonly ApiClient _api;

        public TenenciaModel(ApiClient api)
        {
            _api = api;
        }


        /// <summary>
        /// Id de la tenencia para mirar en el url
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        /// <summary>
        /// Si finalizo bien (caso transito)
        /// </summary>
        [BindProperty]
        public bool? FinalizoBien { get; set; }

        /// <summary>
        /// Comentario opcional en la devolucion en el caso del transito
        /// </summary>
        [BindProperty]
        public string? ObservacionCierre { get; set; }

        /// <summary>
        /// Tenenecia en si
        /// </summary>
        public TenenciaResponse? Tenencia { get; set; }

        /// <summary>
        /// Los controles de esta tenencia
        /// </summary>
        public List<SeguimientoResponse> Controles { get; set; } = new();

        /// <summary>
        /// Si esta en curso, se peude devolver si es transito (se peude pasar a adopcion si se solicita)
        /// </summary>
        public bool EnCurso => Tenencia?.FechaFinReal == null;


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
            var tenencia = await _api.GetAsync<TenenciaResponse>($"api/Tenencia/{Id}");
            if (!tenencia.Exito || tenencia.Datos == null)
                return;

            Tenencia = tenencia.Datos;

            //filtra por mascota y despues las tenencias de esa mascota
            var controles = await _api.GetAsync<List<SeguimientoResponse>>($"api/Seguimiento?mascotaId={Tenencia.MascotaId}");
            Controles = (controles.Datos ?? new()).Where(c => c.TenenciaId == Id).ToList();
        }

        /// <summary>
        /// Registra que el animal volvio al refugio y si hay observacion como termino (para trasnsitos generalmente)
        /// </summary>
        public async Task<IActionResult> OnPostDevolverAsync()
        {
            //se fija si finalizo bien
            if (FinalizoBien == null)
            {
                Error = "Elegí cómo terminó antes de registrar la devolución.";
                return RedirectToPage(new { Id });
            }

            var respuesta = await _api.PatchAsync<object>($"api/Tenencia/{Id}/devolver", new DevolucionRequest
            {
                FinalizoBien = FinalizoBien.Value,
                ObservacionCierre = string.IsNullOrWhiteSpace(ObservacionCierre) ? null : ObservacionCierre.Trim()
            });

            if (respuesta.Exito)
                Mensaje = "Devolución registrada. La mascota volvió a estar en el refugio y los controles pendientes se cancelaron.";
            else
                Error = respuesta.Error;

            return RedirectToPage(new { Id });
        }

        /// <summary>
        /// Pasa un transito a adopcion definitiva, sigue siendo la misma tenencia
        /// </summary>
        public async Task<IActionResult> OnPostConvertirAsync()
        {
            var respuesta = await _api.PatchAsync<object>($"api/Tenencia/{Id}/convertir");

            if (respuesta.Exito)
                Mensaje = "El tránsito pasó a ser una adopción.";
            else
                Error = respuesta.Error;

            return RedirectToPage(new { Id });
        }

    }
}
