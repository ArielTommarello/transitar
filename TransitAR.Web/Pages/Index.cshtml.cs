using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;

        public IndexModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// fila por tarjeta
        /// </summary>
        private const int PorFila = 4;

        /// <summary>
        /// Todas las publicaciones activas
        /// </summary>
        public List<PublicacionPublicaResponse> Todas { get; set; } = new();


        /// <summary>
        /// Filtros botones de especie
        /// </summary>
        public List<EspecieResponse> Especies { get; set; } = new();

        /// <summary>
        /// Error
        /// </summary>
        public string? Error { get; set; }

        //Lista para las filas de la pagina home
        public List<PublicacionPublicaResponse> Transitos =>
            Todas.Where(p => p.Tipo == TipoPublicacion.Transito).Take(PorFila).ToList();

        public List<PublicacionPublicaResponse> Adopciones =>
            Todas.Where(p => p.Tipo == TipoPublicacion.Adopcion).Take(PorFila).ToList();

        public List<PublicacionPublicaResponse> Recientes =>
            Todas.Take(PorFila).ToList();


        public async Task OnGetAsync()
        {
            var especies = await _api.GetAsync<List<EspecieResponse>>("api/Especie");
            Especies = especies.Datos ?? new();

            var respuesta = await _api.GetAsync<List<PublicacionPublicaResponse>>("api/Publicacion/activas");

            if (!respuesta.Exito)
            {
                Error = respuesta.Error;
                return;
            }

            Todas = respuesta.Datos ?? new();
        }
    }
}
