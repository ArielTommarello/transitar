using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures.Requests;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Postulante
{

    /// <summary>
    /// Animales que el postulante tuvo o tiene. Pueden ser adopciones o transito
    /// </summary>
    public class TenenciasModel : PageModel
    {
        private readonly ApiClient _api;

        public TenenciasModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Adopciones y transitos en curso (que tiene el postulante)
        /// </summary>
        public List<HistorialTenenciaResponse> ConVos { get; set; } = new();

        /// <summary>
        /// Transitos devueltos y adopciones devueltas (que tuvo el postulante)
        /// </summary>
        public List<HistorialTenenciaResponse> Anteriores { get; set; } = new();

        /// <summary>
        /// Error de la API
        /// </summary>
        public string? Error { get; set; }

        public async Task OnGetAsync()
        {
            var respuesta = await _api.GetAsync<List<HistorialTenenciaResponse>>("api/Postulacion/tenencias");

            if (!respuesta.Exito || respuesta.Datos == null)
            {
                Error = respuesta.Error;
                return;
            }

            //la api la manda ordenada
            ConVos = respuesta.Datos.Where(t => t.EnCurso).ToList();
            Anteriores = respuesta.Datos.Except(ConVos).ToList();
        }
    }
}
