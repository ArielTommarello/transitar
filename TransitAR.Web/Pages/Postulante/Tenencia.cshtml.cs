using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures.Requests;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Postulante
{
    public class TenenciaModel : PageModel
    {
        private readonly ApiClient _api;

        public TenenciaModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// La tenencia en si, viene null si no existe  o es de otra persona
        /// </summary>
        public HistorialTenenciaResponse? Tenencia { get; set; }

        /// <summary>
        /// La amscota para mostrar en la ficha
        /// </summary>
        public MascotaResponse? Mascota { get; set; }

        /// <summary>
        /// Controles que se hicieron para  esta tenencia
        /// </summary>
        public List<SeguimientoResponse> Controles { get; set; } = new();

        /// <summary>
        /// Proximo control (lo ponemos aparte para poder mostrarlo)
        /// </summary>
        public SeguimientoResponse? ProximoControl { get; set; }

        public async Task OnGetAsync(Guid id)
        {
            //busco la mascota
            var mascota = await _api.GetAsync<MascotaResponse>($"api/Postulacion/tenencias/{id}/mascota");
            if (!mascota.Exito || mascota.Datos == null)
                return;

            Mascota = mascota.Datos;

            //busco la tenencia
            var tenencias = await _api.GetAsync<List<HistorialTenenciaResponse>>("api/Postulacion/tenencias");
            Tenencia = tenencias.Datos?.FirstOrDefault(t => t.Id == id);

            //busco los controles (seguimientos) y filtro por los del usuario
            var controles = await _api.GetAsync<List<SeguimientoResponse>>("api/Postulacion/seguimientos");
            Controles = (controles.Datos ?? new())
                .Where(c => c.TenenciaId == id)
                .OrderByDescending(c => c.FechaProgramada)
                .ToList();

            //ordeno los controles pro el mas proximo
            ProximoControl = Controles
                .Where(c => c.Estado == EstadoSeguimiento.Pendiente)
                .OrderBy(c => c.FechaProgramada)
                .FirstOrDefault();
        }
    }
}
