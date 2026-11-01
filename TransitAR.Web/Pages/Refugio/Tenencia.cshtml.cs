using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures.Requests;
using TransitAR.Structures;
using TransitAR.Web.Services;
using TransitAR.Web.Helpers;

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
        /// Dia del control nuevo
        /// </summary>
        [BindProperty]
        public DateTime? FechaControl { get; set; }

        /// <summary>
        /// Comentario al resolver un control 
        /// </summary>
        [BindProperty]
        public string? ComentarioControl { get; set; }

        /// <summary>
        /// Dia nuevo (cuando reprogramo)
        /// </summary>
        [BindProperty]
        public DateTime? FechaReprogramada { get; set; }


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


        //propiedades calculadas

        /// <summary>
        /// Controles por hacer
        /// </summary>
        public List<SeguimientoResponse> Pendientes => Controles.Where(c => c.Estado == EstadoSeguimiento.Pendiente).ToList();

        /// <summary>
        /// Controles ya resueltos (realizados, cancelados o reprogramados)
        /// </summary>
        public List<SeguimientoResponse> Resueltos => Controles.Where(c => c.Estado != EstadoSeguimiento.Pendiente).ToList();

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


        /// <summary>
        /// Programa un control nuevo para esta tenencia
        /// </summary>
        public async Task<IActionResult> OnPostProgramarAsync()
        {
            if (FechaControl == null)
            {
                Error = "Elige el día del control.";
                return RedirectToPage(new { Id });
            }

            var respuesta = await _api.PostAsync<object>("api/Seguimiento", new SeguimientoRequest
            {
                TenenciaId = Id,                
                FechaProgramada = DateTime.SpecifyKind(FechaControl.Value.Date, DateTimeKind.Unspecified)
            });

            if (respuesta.Exito)
                Mensaje = $"Control programado para el {Formato.Dia(FechaControl.Value)}.";
            else
                Error = respuesta.Error;

            return RedirectToPage(new { Id });
        }


        /// <summary>
        /// Marca un control como hecho
        /// </summary>
        public Task<IActionResult> OnPostRealizadoAsync(Guid controlId) =>
            ResolverAsync(controlId, EstadoSeguimiento.Realizado, "Control marcado como realizado.");

        /// <summary>
        /// Cancela un control que no se va a hacer
        /// </summary>
        public Task<IActionResult> OnPostCancelarAsync(Guid controlId) =>
            ResolverAsync(controlId, EstadoSeguimiento.Cancelado, "Control cancelado.");

        /// <summary>
        /// Reprograma un control, dejo el viejo como historial
        /// </summary>
        public Task<IActionResult> OnPostReprogramarAsync(Guid controlId)
        {
            if (FechaReprogramada == null)
            {
                Error = "Para reprogramar, elegí el día nuevo.";
                return Task.FromResult<IActionResult>(RedirectToPage(new { Id }));
            }

            return ResolverAsync(controlId, EstadoSeguimiento.Reprogramado,
                $"Control reprogramado para el {Formato.Dia(FechaReprogramada.Value)}.");
        }

        //Resuelvo u seguimiento programado
        private async Task<IActionResult> ResolverAsync(Guid controlId, EstadoSeguimiento estado, string aviso)
        {
            var respuesta = await _api.PatchAsync<object>($"api/Seguimiento/{controlId}", new SeguimientoRequest
            {
                Estado = estado,
                Observacion = string.IsNullOrWhiteSpace(ComentarioControl) ? null : ComentarioControl.Trim(),
                FechaProgramada = estado == EstadoSeguimiento.Reprogramado && FechaReprogramada != null
                    ? DateTime.SpecifyKind(FechaReprogramada.Value.Date, DateTimeKind.Unspecified)
                    : null
            });

            if (respuesta.Exito)
                Mensaje = aviso;
            else
                Error = respuesta.Error;

            return RedirectToPage(new { Id });
        }

    }
}
