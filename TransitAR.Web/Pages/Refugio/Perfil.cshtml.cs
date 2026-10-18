using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures.Requests;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Refugio
{
    /// <summary>
    /// Perfil del refugio, practiamente igual al del postulante
    /// </summary>
    public class PerfilModel : PageModel
    {
        private readonly ApiClient _api;

        public PerfilModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Datos del refugio mismo DTO que recibe la API
        /// </summary>
        [BindProperty]
        public RefugioRequest Datos { get; set; } = new();

        /// <summary>
        /// Las cinco redes  una por tipo
        /// </summary>
        [BindProperty]
        public Dictionary<TipoContacto, string?> Redes { get; set; } = new();

        /// <summary>
        /// Foto  del refugio
        /// </summary>
        [BindProperty]
        public IFormFile? Foto { get; set; }

        /// <summary>
        /// Si ya tiene una foto
        /// </summary>
        [BindProperty]
        public bool TieneFoto { get; set; }

        /// <summary>
        /// True si todavia no completo la descripcion (muestro un aviso)
        /// </summary>
        public bool FaltaCompletar { get; set; }

        /// <summary>
        /// Error de la API
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Aviso despues de guardar
        /// </summary>
        [TempData]
        public string? Mensaje { get; set; }

        /// <summary>
        /// Direccion del logo en la API
        /// </summary>
        public string UrlFoto => _api.UrlFotoRefugio(Guid.Parse(User.FindFirst("refugioId")!.Value));

        public async Task OnGetAsync()
        {
            var respuesta = await _api.GetAsync<RefugioResponse>("api/Refugio");

            if (!respuesta.Exito || respuesta.Datos == null)
            {
                Error = respuesta.Error;
                return;
            }

            CargarDesde(respuesta.Datos);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            //de las cinco redes solo se muestran las que estan
            Datos.Contactos = Redes
                .Where(r => !string.IsNullOrWhiteSpace(r.Value))
                .Select(r => new ContactoRequest { Tipo = r.Key, Url = r.Value!.Trim() })
                .ToList();

            var guardado = await _api.PutAsync<RefugioResponse>("api/Refugio", Datos);

            if (!guardado.Exito)
            {
                Error = guardado.Error;
                return Page();
            }

            //foto si subio una
            if (Foto != null)
            {
                var foto = await _api.PutArchivoAsync<object>("api/Refugio/foto", Foto, "foto");

                //error con la foto (guardo el perfil igual)
                if (!foto.Exito)
                {
                    Mensaje = $"Los datos se guardaron, pero el logo no: {foto.Error}";
                    return RedirectToPage();
                }
            }

            Mensaje = "Perfil guardado.";
            return RedirectToPage();
        }

        /// <summary>
        /// Paso lo que devuelve la API al formulario
        /// </summary>
        private void CargarDesde(RefugioResponse r)
        {
            Datos = new RefugioRequest
            {
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                Email = r.Email,
                Telefono = r.Telefono,
                Direccion = r.Direccion,
                Localidad = r.Localidad,
                MensajeRechazoAutomatico = r.MensajeRechazoAutomatico
            };

            Redes = r.Contactos.ToDictionary(c => c.Tipo, c => (string?)c.Url);
            TieneFoto = r.FotoPerfil;
            FaltaCompletar = string.IsNullOrWhiteSpace(r.Descripcion);
        }
    }
}
