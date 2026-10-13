using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures.Requests;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Postulante
{
    public class PostulacionesModel : PageModel
    {
        private readonly ApiClient _api;

        public PostulacionesModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Pendientes, En espera y aceptadas
        /// </summary>
        public List<PostulacionResponse> EnCurso { get; set; } = new();

        /// <summary>
        /// REchazadas y Finalizadas o Retiradas
        /// </summary>
        public List<PostulacionResponse> Finalizadas { get; set; } = new();

        /// <summary>
        /// Mensaje en caso de que se retiro
        /// </summary>
        [TempData]
        public string? Mensaje { get; set; }

        /// <summary>
        /// Error de la API (se redirige despues)
        /// </summary>
        [TempData]
        public string? Error { get; set; }

        public async Task OnGetAsync()
        {
            var respuesta = await _api.GetAsync<List<PostulacionResponse>>("api/Postulacion");

            //error en datos
            if (!respuesta.Exito || respuesta.Datos == null)
            {
                Error = respuesta.Error;
                return;
            }

            //separo, ya tengo orden en la api (puede ser adopcion o transito asignado en algun momento)
            EnCurso = respuesta.Datos
                 .Where(p => p.Estado == EstadoPostulacion.Pendiente
                          || p.Estado == EstadoPostulacion.EnEspera
                          || (p.Estado == EstadoPostulacion.Aceptada && !p.TieneTenencia))
                 .ToList();

            //finalizadsas, todas las que no son las encurso
            Finalizadas = respuesta.Datos.Except(EnCurso).ToList();
        }

        /// <summary>
        /// Retira una postulacion (tiene que estar en evaluacion o espera)
        /// </summary>
        public async Task<IActionResult> OnPostRetirarAsync(Guid id)
        {
            var respuesta = await _api.PatchAsync<object>($"api/Postulacion/{id}/retirar");

            if (respuesta.Exito)
                Mensaje = "Te retiraste de la postulación. Si cambiás de idea, podés volver a postularte desde la publicación de esta mascota.";
            else
                Error = respuesta.Error;

            //RedirectToPage(EFCORE)
            return RedirectToPage();
        }

    }
}
