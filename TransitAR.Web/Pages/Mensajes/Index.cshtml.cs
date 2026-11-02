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
            //que no quede en cache asi no me quedan los viejos cuando vuelvo apra atras y peudo actualziar
            Response.Headers.CacheControl = "no-store";

            var respuesta = await _api.GetAsync<List<ChatResumenResponse>>(
                $"api/Mensaje/chats?publicacionId={PublicacionId}&mascotaId={MascotaId}");

            if (!respuesta.Exito || respuesta.Datos == null)
            {
                Error = respuesta.Error;
                return;
            }

            //chats cerrados solo lectura, y primeros los no leidos como wpp
            Chats = respuesta.Datos                
                .Where(c => c.PuedeEscribir || c.FechaUltimoMensaje != null)                
                .OrderByDescending(c => c.NoLeidos > 0)
                .ThenByDescending(c => c.FechaUltimoMensaje ?? DateTime.MinValue)
                .ToList();
        }
    }
}
