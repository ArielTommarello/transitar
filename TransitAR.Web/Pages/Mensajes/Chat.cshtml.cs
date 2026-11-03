using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Mensajes
{
    /// <summary>
    /// Conversacion o chat entre postulacion y refugio, se ve desde cada rol. Actualizacon cada 15 segundos
    /// </summary>
    public class ChatModel : PageModel
    {
        private readonly ApiClient _api;

        public ChatModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// La postulacion en si
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public Guid PostulacionId { get; set; }

        /// <summary>
        /// Texto del mensaje nuevo
        /// </summary>
        [BindProperty]
        public string? Texto { get; set; }

        /// <summary>
        /// La conversacion completa, null si no existe o no es suya
        /// </summary>
        public ChatResponse? Chat { get; set; }

        /// <summary>
        /// Error
        /// </summary>
        [TempData]
        public string? Error { get; set; }


        public async Task OnGetAsync()
        {
            //el boton no guarda en cache la conversacion cuando se vuelve atras
            Response.Headers.CacheControl = "no-store";

            Chat = await CargarAsync();
        }

        /// <summary>
        /// Carga la lista de mensajes , para actualizar cada 15 segundos
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnGetMensajesAsync()
        {
            Response.Headers.CacheControl = "no-store";

            var chat = await CargarAsync();
            if (chat == null)
                return NotFound();

            return Partial("_Mensajes", chat.Mensajes);
        }

        /// <summary>
        /// Envia un mensaje
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPostEnviarAsync()
        {
            if (string.IsNullOrWhiteSpace(Texto))
            {
                Error = "Escribe un mensaje antes de enviar.";
                return RedirectToPage(new { postulacionId = PostulacionId });
            }

            var respuesta = await _api.PostFormularioAsync<object>(
                $"api/Mensaje/{PostulacionId}",
                new Dictionary<string, string?> { ["Texto"] = Texto.Trim() },
                new List<IFormFile>(),
                "archivos");

            if (!respuesta.Exito)
                Error = respuesta.Error;

            return RedirectToPage(new { postulacionId = PostulacionId });
        }

        /// <summary>
        /// carga siempre para la pagina y la actualizacion de 15 segundos
        /// </summary>
        /// <returns></returns>
        private async Task<ChatResponse?> CargarAsync()
        {
            var respuesta = await _api.GetAsync<ChatResponse>($"api/Mensaje/{PostulacionId}");
            return respuesta.Exito ? respuesta.Datos : null;
        }
    }
}
