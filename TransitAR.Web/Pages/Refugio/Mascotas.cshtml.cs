using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Refugio
{
    public class MascotasModel : PageModel
    {

        private readonly ApiClient _api;

        public MascotasModel(ApiClient api)
        {
            _api = api;
        }


        /// <summary>
        /// Filtro elegido,si esta null muestra todo sin filtro
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public EstadoMascota? Estado { get; set; }

        /// <summary>
        /// Todas las mascotas del refugio, asi dsps saco cuantos tengo en cada estado
        /// </summary>
        public List<MascotaResponse> Todas { get; set; } = new();

        /// <summary>
        /// Las mascotas que se muestran segun el filtro elegido
        /// </summary>
        public List<MascotaResponse> Mascotas { get; set; } = new();


        /// <summary>
        /// lo que muestro despues de borrar 
        /// </summary>
        [TempData]
        public string? Mensaje { get; set; }

        /// <summary>
        /// Error de la API (TempDAta despues redirijo)
        /// </summary>
        [TempData]
        public string? Error { get; set; }


        public async Task OnGetAsync()
        {
            var respuesta = await _api.GetAsync<List<MascotaResponse>>("api/Mascota");

            if (!respuesta.Exito || respuesta.Datos == null)
            {
                Error = respuesta.Error;
                return;
            }

            Todas = respuesta.Datos.OrderBy(m => m.Nombre).ToList();

            //aplico el filtro, la API devuelve todas las del refugio
            Mascotas = Estado == null
                ? Todas
                : Todas.Where(m => m.Estado == Estado).ToList();
        }

        /// <summary>
        /// Borra una mascota que cargaron por error. No se peude si hay tenencias o publicaciones.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostEliminarAsync(Guid id)
        {
            var respuesta = await _api.DeleteAsync<object>($"api/Mascota/{id}");

            if (respuesta.Exito)
                Mensaje = "Mascota borrada.";
            else
                Error = respuesta.Error;

            //vuelve con el mismo filtro que tenia antes
            return RedirectToPage(new { estado = Estado });
        }

        /// <summary>
        /// Cuantas tengo por estado (lo uso en el filtro)
        /// </summary>
        /// <param name="estado"></param>
        /// <returns></returns>
        public int Cantidad(EstadoMascota? estado) =>
            estado == null ? Todas.Count : Todas.Count(m => m.Estado == estado);
    }
}
