using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Structures.Requests;
using TransitAR.Web.Services;
using TransitAR.Web.Helpers;

namespace TransitAR.Web.Pages.Refugio
{
    /// <summary>
    /// Agenda de seguimiento que ve el refugio, controles pendientes de las tenencias
    /// </summary>
    public class SeguimientoModel : PageModel
    {
        private readonly ApiClient _api;

        public SeguimientoModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Todos los controles pendientes
        /// </summary>
        public List<SeguimientoResponse> Pendientes { get; set; } = new();

        /// <summary>
        /// Error
        /// </summary>
        [TempData]
        public string? Error { get; set; }


        //propiedades calculadas

        /// <summary>
        /// Los que ya se pasaron de fecha
        /// </summary>
        public List<SeguimientoResponse> Atrasados => Pendientes.Where(c => c.Vencido).ToList();

        /// <summary>
        /// Los que todavia estan a tiempo
        /// </summary>
        public List<SeguimientoResponse> Proximos => Pendientes.Where(c => !c.Vencido).ToList();

        /// <summary>
        /// Los proximos que son hoy o mañana (cartel de arriba)
        /// </summary>
        public List<SeguimientoResponse> ProximosDias => Proximos.Where(c => EsHoy(c)).ToList();

        public async Task OnGetAsync()
        {
            var respuesta = await _api.GetAsync<List<SeguimientoResponse>>($"api/Seguimiento?estado={(int)EstadoSeguimiento.Pendiente}");

            if (!respuesta.Exito || respuesta.Datos == null)
            {
                Error = respuesta.Error;
                return;
            }

            Pendientes = respuesta.Datos;
        }


        /// <summary>
        /// Si el control es hoy o mañana
        /// </summary>
        public static bool EsHoy(SeguimientoResponse c) =>
            c.FechaProgramada.Date <= Formato.HoyEnArgentina().AddDays(1);

        /// <summary>
        /// Texto de cuadno es, si hoy o mañana
        /// </summary>
        public static string? Cuando(SeguimientoResponse c) =>
            c.FechaProgramada.Date == Formato.HoyEnArgentina() ? "Hoy"
            : c.FechaProgramada.Date == Formato.HoyEnArgentina().AddDays(1) ? "Mañana"
            : null;
    }
}
