using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Structures.Requests;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Publicaciones
{
    /// <summary>
    /// Detalle de una publicacion. Aca esta el boton para postularse
    /// </summary>
    public class DetalleModel : PageModel
    {
        private readonly ApiClient _api;

        public DetalleModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// La publicacion, null si no existe o ya no esta activa
        /// </summary>
        public PublicacionPublicaResponse? Publicacion { get; set; }

        /// <summary>
        /// Postulacion del usuario logueado a esta publicacion puede ser null si no se postulo
        /// </summary>
        public PostulacionResponse? MiPostulacion { get; set; }

        /// <summary>
        /// Error rechazo del postulante
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Mensaje luego de la postulacion
        /// </summary>
        [TempData]
        public string? Mensaje { get; set; }

        public async Task OnGetAsync(Guid id)
        {
            await CargarAsync(id);
        }

        /// <summary>
        /// Postulacion a una publicacion activa (adopcion o transito)
        /// </summary>
        /// <param name="id"></param>
        public async Task<IActionResult> OnPostPostularAsync(Guid id)
        {
            var postulacion = new PostulacionRequest
            {
                PublicacionId = id
            };

            var respuesta = await _api.PostAsync<object>("api/Postulacion", postulacion);

            if (!respuesta.Exito)
            {
                //errores (sin perfil, cupo lleno, ya postulado o otros)
                Error = respuesta.Error;
                await CargarAsync(id);
                return Page();
            }

            //Arreglo para en caso de que redireccione y no se postule dos veces
            Mensaje = "Ya te postulaste a esta publicacion. Podés seguir el estado en Mis postulaciones.";
            return RedirectToPage(new { id });
        }

        /// <summary>
        /// Cargo la publicacion (detalle) y si es postulante la postualcion
        /// </summary>
        /// <param name="id"></param>
        private async Task CargarAsync(Guid id)
        {
            var respuesta = await _api.GetAsync<PublicacionPublicaResponse>($"api/Publicacion/activas/{id}");
            Publicacion = respuesta.Exito ? respuesta.Datos : null;

            //si es postulante busco si ya se postulo para bloquear el boton
            if (Publicacion != null && User.IsInRole(nameof(RolUsuario.Usuario)))
            {
                var mias = await _api.GetAsync<List<PostulacionResponse>>("api/Postulacion");
                MiPostulacion = mias.Exito
                    ? mias.Datos!.FirstOrDefault(m => m.PublicacionId == id)
                    : null;
            }
        }
    }
}