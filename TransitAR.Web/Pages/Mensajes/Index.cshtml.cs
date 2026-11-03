using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Mensajes
{
    /// <summary>
    /// Index lista de chats, misma para refugio y postulante (manejo por token/rol)
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;

        public IndexModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Filtro opcional por publicacion 
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public Guid? PublicacionId { get; set; }

        /// <summary>
        /// Filtro opcional por mascota 
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public Guid? MascotaId { get; set; }

        /// <summary>
        /// Chats para mostrar
        /// </summary>
        public List<ChatResumenResponse> Chats { get; set; } = new();

        /// <summary>
        /// Error
        /// </summary>
        [TempData]
        public string? Error { get; set; }


        //propiedades calculadas

        /// <summary>
        /// Filtrada  para mostrar el aviso con "Ver todos"
        /// </summary>
        public bool Filtrada => PublicacionId != null || MascotaId != null;

        /// <summary>
        /// Nombre de la mascota del filtro
        /// </summary>
        public string? MascotaNombre => Filtrada ? Chats.FirstOrDefault()?.MascotaNombre : null;


        public async Task OnGetAsync()
        {
            //qpara que atras no storee en cache
            Response.Headers.CacheControl = "no-store";

            Error = await CargarAsync();
        }

        /// <summary>
        /// solo muestro la lista, y la actualzio 15s
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetChatsAsync()
        {
            Response.Headers.CacheControl = "no-store";

            if (await CargarAsync() != null)
                return NotFound();

            return Partial("_Chats", Chats);
        }

        /// <summary>
        /// cargo para la pagina entera y cada actualziacion
        /// </summary>
        /// <returns></returns>
        private async Task<string?> CargarAsync()
        {
            var respuesta = await _api.GetAsync<List<ChatResumenResponse>>(
                $"api/Mensaje/chats?publicacionId={PublicacionId}&mascotaId={MascotaId}");

            if (!respuesta.Exito || respuesta.Datos == null)
                return respuesta.Error ?? "No pudimos traer las conversaciones.";

            //cerrado solo lectura, y dsps pongo los no leido primero como wpp
            Chats = respuesta.Datos                
                .Where(c => c.PuedeEscribir || c.FechaUltimoMensaje != null)                
                .OrderByDescending(c => c.NoLeidos > 0)
                .ThenByDescending(c => c.FechaUltimoMensaje ?? DateTime.MinValue)
                .ToList();

            return null;
        }

    }
}
