using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Publicaciones
{
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;

        public IndexModel(ApiClient api)
        {
            _api = api;
        }

        //los filtros, leidos de la URL (get formulario)
        [BindProperty(SupportsGet = true)]
        public TipoPublicacion? Tipo { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? EspecieId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Tamanio? Tamanio { get; set; }

        [BindProperty(SupportsGet = true)]
        public Sexo? Sexo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Ubicacion { get; set; }

        /// <summary>
        /// Las publicaciones que cumplen los filtros
        /// </summary>
        public List<PublicacionPublicaResponse> Publicaciones { get; set; } = new();

        /// <summary>
        /// Para el desplegable de especies
        /// </summary>
        public List<EspecieResponse> Especies { get; set; } = new();

        public string? Error { get; set; }

        public async Task OnGetAsync()
        {
            var especies = await _api.GetAsync<List<EspecieResponse>>("api/Especie");
            Especies = especies.Datos ?? new();

            //solo se mandan los filtros que la persona completo 
            var filtros = new Dictionary<string, string?>();

            if (Tipo != null)
                filtros["tipo"] = ((int)Tipo.Value).ToString();
            if (EspecieId != null)
                filtros["especieId"] = EspecieId.Value.ToString();
            if (Tamanio != null)
                filtros["tamanio"] = ((int)Tamanio.Value).ToString();
            if (Sexo != null)
                filtros["sexo"] = ((int)Sexo.Value).ToString();
            if (!string.IsNullOrWhiteSpace(Ubicacion))
                filtros["ubicacion"] = Ubicacion.Trim();

            var ruta = QueryHelpers.AddQueryString("api/Publicacion/activas", filtros);
            var respuesta = await _api.GetAsync<List<PublicacionPublicaResponse>>(ruta);

            if (!respuesta.Exito)
            {
                Error = respuesta.Error;
                return;
            }

            Publicaciones = respuesta.Datos ?? new();
        }
    }
}
