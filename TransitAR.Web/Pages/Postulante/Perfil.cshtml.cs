using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Postulante
{
    public class PerfilModel : PageModel
    {
        private readonly ApiClient _api;
        

        public PerfilModel(ApiClient api)
        {
            _api = api;            
        }

        /// <summary>
        /// Datos del perfil mismo DTO que recibe de la API
        /// </summary>
        [BindProperty]
        public PerfilRequest Datos { get; set; } = new();

        /// <summary>
        /// Las cinco redes  una por tipo
        /// </summary>
        [BindProperty]
        public Dictionary<TipoContacto, string?> Redes { get; set; } = new();

        /// <summary>
        /// Foto  del postulante
        /// </summary>
        [BindProperty]
        public IFormFile? Foto { get; set; }

        /// <summary>
        /// True si todavia no tiene perfil (auxiliar) , para poder llevarlo a donde se completa el perfil
        /// </summary>
        [BindProperty]
        public bool EsNuevo { get; set; }

        /// <summary>
        /// Si ya tiene una foto
        /// </summary>
        [BindProperty]
        public bool TieneFoto { get; set; }

        /// <summary>
        /// Return to para ver donde vuelve, mucho uso , para redireccionar en caso de estar en publicacion
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

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
        /// Direccion de la foto en la API (se arma en apiclient)
        /// </summary>
        public string UrlFoto => _api.UrlFotoPostulante(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));


        public async Task OnGetAsync()
        {
            var respuesta = await _api.GetAsync<PerfilResponse>("api/Perfil");

            //todavia no tiene perfil se muestra vacio para completarlo (404 da el error)
            if (respuesta.Estado == StatusCodes.Status404NotFound)
            {
                EsNuevo = true;
                return;
            }

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

            //completar el perfil primero 
            var guardado = EsNuevo
                ? await _api.PostAsync<PerfilResponse>("api/Perfil", Datos)
                : await _api.PutAsync<PerfilResponse>("api/Perfil", Datos);

            if (!guardado.Exito)
            {
                Error = guardado.Error;
                return Page();
            }

            //foto si subio una
            if (Foto != null)
            {
                var foto = await _api.PutArchivoAsync<object>("api/Perfil/foto", Foto, "foto");

                //error con la foto (guardo el perfil igual)
                if (!foto.Exito)
                {
                    Mensaje = $"Tu perfil se guardó, pero la foto no: {foto.Error}";
                    return RedirectToPage();
                }
            }

            //redireccion (genrealmente publciaciones)
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            Mensaje = "Perfil guardado.";
            return RedirectToPage();
        }

        /// <summary>
        /// Paso lo que devuelve la API al formulario 
        /// </summary>
        private void CargarDesde(PerfilResponse p)
        {
            Datos = new PerfilRequest
            {
                Seleccion = p.Seleccion,
                TipoVivienda = p.TipoVivienda,
                TienePatio = p.TienePatio,
                PatioCerrado = p.PatioCerrado,
                TieneOtrasMascotas = p.TieneOtrasMascotas,
                DetalleOtrasMascotas = p.DetalleOtrasMascotas,
                HorasSoloPorDia = p.HorasSoloPorDia,
                DisponibilidadHorario = p.DisponibilidadHorario,
                CercaniaVeterinaria = p.CercaniaVeterinaria,
                ExperienciaPrevia = p.ExperienciaPrevia,
                MotivoPostulacion = p.MotivoPostulacion
            };

            Redes = p.Contactos.ToDictionary(c => c.Tipo, c => (string?)c.Url);
            TieneFoto = p.FotoPerfil;
        }


    }
}
