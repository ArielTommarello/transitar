using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Refugio
{
    /// <summary>
    /// Pestañas que se ven en el lsitado (vencidas tambien en curso) , lista propia, porque la tenencia se repite en 2 a la vez capaz
    /// </summary>
    public enum VistaTenencias
    {
        EnCurso,
        Vencidas,
        SinControles,
        Finalizadas
    }


    /// <summary>
    /// Animales que tienen tenencia (transito o adopcion) con pestañas , en un refugio
    /// </summary>
    public class TenenciasModel : PageModel
    {

        private readonly ApiClient _api;

        public TenenciasModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Pestaña que se eligio, arranca siempre en curso
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public VistaTenencias Vista { get; set; } = VistaTenencias.EnCurso;

        /// <summary>
        /// Tnenecia de la masctoa en si (viene por direccion)
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public Guid? MascotaId { get; set; }

        /// <summary>
        /// Todas las tenencias (cuento las pestañas)
        /// </summary>
        public List<TenenciaResponse> Todas { get; set; } = new();

        /// <summary>
        /// Tenencias de la pestaña que se eligio
        /// </summary>
        public List<TenenciaResponse> Tenencias { get; set; } = new();

        /// <summary>
        /// Nombre de la mascota filtrada
        /// </summary>
        public string? MascotaNombre => MascotaId == null ? null : Todas.FirstOrDefault()?.MascotaNombre;


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
            var respuesta = await _api.GetAsync<List<TenenciaResponse>>("api/Tenencia");

            if (!respuesta.Exito || respuesta.Datos == null)
            {
                Error = respuesta.Error;
                return;
            }

            //ya ordenadas
            Todas = MascotaId == null
                ? respuesta.Datos
                : respuesta.Datos.Where(t => t.MascotaId == MascotaId).ToList();

            Tenencias = Todas.Where(t => EnVista(t, Vista)).ToList();
        }

        /// <summary>
        /// Si una tenencia va en una pestaña
        /// </summary>
        public static bool EnVista(TenenciaResponse t, VistaTenencias vista) => vista switch
        {
            VistaTenencias.EnCurso => t.FechaFinReal == null,
            VistaTenencias.Vencidas => t.EstaVencida,
            VistaTenencias.SinControles => t.FechaFinReal == null && !t.TieneControlPendiente,
            _ => t.FechaFinReal != null
        };

        /// <summary>
        /// Cantidad en un apestaña
        /// </summary>
        public int Cantidad(VistaTenencias vista) => Todas.Count(t => EnVista(t, vista));

    }
}
