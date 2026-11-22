using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Refugio
{
    /// <summary>
    /// Postualciones que se recibieron en la publicacion, con minimo resumen del postulante para comparar. Tiene detalle para decidir
    /// </summary>
    public class PostulantesModel : PageModel
    {

        private readonly ApiClient _api;

        public PostulantesModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Publciacion en si
        /// </summary>
        public PublicacionResponse? Publicacion { get; set; }

        /// <summary>
        /// Postulacion aceptada
        /// </summary>
        public PostulacionRefugioResponse? Aceptada { get; set; }

        /// <summary>
        /// Pendientes y en espera (si se cancelan se pueden elegir luego)
        /// </summary>
        public List<PostulacionRefugioResponse> EnEvaluacion { get; set; } = new();

        /// <summary>
        /// Rechazadas y retiradas
        /// </summary>
        public List<PostulacionRefugioResponse> Finalizadas { get; set; } = new();


        // <summary>
        /// Filtro: false = en evaluacion (arranca asi), true = finalizadas
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public bool VerFinalizadas { get; set; }

        /// <summary>
        /// Las del filtro elegido, paginadas
        /// </summary>
        public PaginatedList<PostulacionRefugioResponse> Postulaciones { get; set; } = default!;

        /// <summary>
        /// True si no es del refugio
        /// </summary>
        public bool NoEncontrada { get; set; }


        public async Task OnGetAsync(Guid id, int? pageIndex)
        {
            var publicacion = await _api.GetAsync<PublicacionResponse>($"api/Publicacion/{id}");
            if (!publicacion.Exito || publicacion.Datos == null)
            {
                NoEncontrada = true;
                return;
            }

            Publicacion = publicacion.Datos;

            var respuesta = await _api.GetAsync<List<PostulacionRefugioResponse>>($"api/Publicacion/{id}/postulaciones");
            var todas = (respuesta.Datos ?? new())
                .OrderBy(p => p.FechaPostulacion)
                .ToList();

            Aceptada = todas.FirstOrDefault(p => p.Estado == EstadoPostulacion.Aceptada);

            EnEvaluacion = todas
                .Where(p => p.Estado == EstadoPostulacion.Pendiente || p.Estado == EstadoPostulacion.EnEspera)
                .ToList();

            Finalizadas = todas
                .Where(p => p.Estado == EstadoPostulacion.Rechazada || p.Estado == EstadoPostulacion.Retirada)
                .ToList();

            var filtradas = VerFinalizadas ? Finalizadas : EnEvaluacion;

            //paginacion como el tutorial de Microsoft, de a 5
            var pageSize = 5;
            Postulaciones = PaginatedList<PostulacionRefugioResponse>.Create(filtradas, pageIndex ?? 1, pageSize);
        }

    }
}
