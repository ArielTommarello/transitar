using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Refugio
{
    /// <summary>
    /// Publicaicones que hace y ve el refugio, en una lista tabla, con filtros (se peude pausar, reanudar , y cerrar)
    /// </summary>
    public class PublicacionesModel : PageModel
    {
        private readonly ApiClient _api;

        public PublicacionesModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Filtro de estado que eligen , default null (todas)
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public EstadoPublicacion? Estado { get; set; }


        /// <summary>
        /// Todas las publicaciones del registro (count estado)
        /// </summary>
        public List<PublicacionResponse> Todas { get; set; } = new();

        /// <summary>
        /// Publicaciones con el filtro mostrando
        /// </summary>
        public List<PublicacionResponse> Publicaciones { get; set; } = new();

        /// <summary>
        /// Aviso cuando cambia un estado
        /// </summary>
        [TempData]
        public string? Mensaje { get; set; }

        /// <summary>
        /// Errror mensaje
        /// </summary>
        [TempData]
        public string? Error { get; set; }


        public async Task OnGetAsync()
        {
            var respuesta = await _api.GetAsync<List<PublicacionResponse>>("api/Publicacion");

            if (!respuesta.Exito || respuesta.Datos == null)
            {
                Error = respuesta.Error;
                return;
            }

            //ya ordenadas pro api
            Todas = respuesta.Datos;
            Publicaciones = Estado == null
                ? Todas
                : Todas.Where(p => p.Estado == Estado).ToList();
        }

        /// <summary>
        /// Cambio el estado , pausa, reanudar, y cerrar. Si cierra hay que crear otra
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nuevoEstado"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostCambiarEstadoAsync(Guid id, EstadoPublicacion nuevoEstado)
        {
            var respuesta = await _api.PatchAsync<object>($"api/Publicacion/{id}/estado/{(int)nuevoEstado}");

            if (respuesta.Exito)
                Mensaje = nuevoEstado switch
                {
                    EstadoPublicacion.Pausada => "Publicación pausada: ya no se ve en el listado público.",
                    EstadoPublicacion.Activa => "Publicación reanudada.",
                    _ => "Publicación cerrada."
                };
            else
                Error = respuesta.Error;

            //vuelve con el mismo filtro estado que tenia
            return RedirectToPage(new { estado = Estado });
        }

        /// <summary>
        /// Total de un estado para mostrar en el filtro
        /// </summary>
        /// <param name="estado"></param>
        /// <returns></returns>
        public int Cantidad(EstadoPublicacion? estado) =>
            estado == null ? Todas.Count : Todas.Count(p => p.Estado == estado);
    }
}
