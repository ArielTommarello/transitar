using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Structures.Requests;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Refugio
{

    /// <summary>
    /// Detalle del postulante, como lo ve el refugio, perfil historial , y botones para aceptar rechazar y dsps confirmar entrega
    /// </summary>
    public class PostulanteModel : PageModel
    {
        private readonly ApiClient _api;

        public PostulanteModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Id de la  publicacion a la que se postulo el postulante
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public Guid PublicacionId { get; set; }


        /// <summary>
        /// Postulacion ddesde donde se mira
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public Guid PostulacionId { get; set; }


        /// <summary>
        /// Observacion al rechazar
        /// </summary>
        [BindProperty]
        public string? Observacion { get; set; }


        /// <summary>
        /// Fecha de devolcuion se usa solo en transito
        /// </summary>
        [BindProperty]
        public DateTime? FechaFinEstimada { get; set; }

        /// <summary>
        /// Publicacion Response y Posutlacion response
        /// </summary>
        public PublicacionResponse? Publicacion { get; set; }
        public PostulacionRefugioResponse? Postulacion { get; set; }



        /// <summary>
        /// Tenencias historal de cualquier refugio
        /// </summary>
        public List<HistorialTenenciaResponse> Historial { get; set; } = new();

        /// <summary>
        /// Controles de esas tnenecias historial
        /// </summary>
        public List<SeguimientoResponse> Controles { get; set; } = new();


        /// <summary>
        /// Contadores del historial , uso propiedades calculadas para no tener que usar gets
        /// </summary>
        public int Devueltas => Historial.Count(t => t.FechaFinReal != null);
        public int ConInconvenientes => Historial.Count(t => t.FinalizoBien == false);
        public int ACargo => Historial.Count(t => t.EnCurso);
        public int Vencidas => Historial.Count(t => t.VencidaEnCurso);

        /// <summary>
        /// COntroles que tuvo la tenencia historial (prop calculada)
        /// </summary>
        /// <param name="tenenciaId"></param>
        /// <returns></returns>
        public List<SeguimientoResponse> ControlesDe(Guid tenenciaId) =>
            Controles.Where(c => c.TenenciaId == tenenciaId).OrderBy(c => c.FechaProgramada).ToList();


        /// <summary>
        /// Publicacion cerrada, no puede tener ociones, la aceptada pasa a ser entregada
        /// </summary>
        public bool PublicacionCerrada => Publicacion?.Estado == EstadoPublicacion.Cerrada;

        /// <summary>
        /// Publciacion no encontrada
        /// </summary>
        public bool NoEncontrada { get; set; }


        /// <summary>
        /// Mensaje
        /// </summary>
        [TempData]
        public string? Mensaje { get; set; }


        /// <summary>
        /// error mensaje
        /// </summary>
        [TempData]
        public string? Error { get; set; }



        public async Task OnGetAsync()
        {
            var publicacion = await _api.GetAsync<PublicacionResponse>($"api/Publicacion/{PublicacionId}");
            if (!publicacion.Exito || publicacion.Datos == null)
            {
                NoEncontrada = true;
                return;
            }
            Publicacion = publicacion.Datos;

            var postulaciones = await _api.GetAsync<List<PostulacionRefugioResponse>>($"api/Publicacion/{PublicacionId}/postulaciones");
            Postulacion = postulaciones.Datos?.FirstOrDefault(p => p.Id == PostulacionId);
            if (Postulacion == null)
            {
                NoEncontrada = true;
                return;
            }

            var historial = await _api.GetAsync<List<HistorialTenenciaResponse>>($"api/Tenencia/postulante/{Postulacion.UsuarioId}");
            Historial = historial.Datos ?? new();

            var controles = await _api.GetAsync<List<SeguimientoResponse>>($"api/Tenencia/postulante/{Postulacion.UsuarioId}/seguimientos");
            Controles = controles.Datos ?? new();

            //en el transito pongo fecha de arranque de dela devolucion desde que sale
            if (Publicacion.Tipo == TipoPublicacion.Transito && Publicacion.PlazoEstimado != null)
                FechaFinEstimada = DateTime.Today.AddDays(Publicacion.PlazoEstimado.Value);
        }


        /// <summary>
        /// Aceptar, se ponene en espera a las demas
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostAceptarAsync()
        {
            var respuesta = await _api.PatchAsync<object>($"api/Publicacion/{PublicacionId}/postulaciones/{PostulacionId}/aceptar");

            if (respuesta.Exito)
                Mensaje = "Postulación aceptada. Las demás quedaron en espera y la publicación se pausó hasta la entrega.";
            else
                Error = respuesta.Error;

            return RedirectToPage(new { PublicacionId, PostulacionId });
        }

        /// <summary>
        /// Rechazo de un postulante mas una observacionq ue epuede ser opcional. en caso de ser la aceptada abro la publi de nuevo
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostRechazarAsync()
        {
            var respuesta = await _api.PatchAsync<object>(
                $"api/Publicacion/{PublicacionId}/postulaciones/{PostulacionId}/rechazar",
                new RechazoRequest { Observacion = string.IsNullOrWhiteSpace(Observacion) ? null : Observacion.Trim() });

            if (respuesta.Exito)
                Mensaje = "Postulación rechazada.";
            else
                Error = respuesta.Error;

            return RedirectToPage(new { PublicacionId, PostulacionId });
        }

        /// <summary>
        /// Confirmaciond e la entrega, creo la tenencia, cierra la publiacaion y rechazo a los demas
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostConfirmarEntregaAsync()
        {
            //dia del calendario
            var fecha = FechaFinEstimada == null
                ? (DateTime?)null
                : DateTime.SpecifyKind(FechaFinEstimada.Value.Date, DateTimeKind.Unspecified);

            var respuesta = await _api.PostAsync<object>("api/Tenencia",
                new TenenciaRequest { PostulacionId = PostulacionId, FechaFinEstimada = fecha });

            if (respuesta.Exito)
                Mensaje = "Entrega confirmada. La publicación se cerró y las demás postulaciones se rechazaron.";
            else
                Error = respuesta.Error;

            return RedirectToPage(new { PublicacionId, PostulacionId });
        }


    }
}
