using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures;
using TransitAR.Structures.Requests;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Refugio
{
    /// <summary>
    /// Detalle de la publciaicon en si, se crea, edita y sirve apra ver.
    /// </summary>
    public class PublicacionModel : PageModel
    {

        private readonly ApiClient _api;

        public PublicacionModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Id de la publicaion, para editar o ver.Null es nueva
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public Guid? Id { get; set; }

        /// <summary>
        /// Ide de una publciacion cerrada para poder copiar los datos
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public Guid? Reusar { get; set; }

        /// <summary>
        /// Datos de la publicaicon , DTO
        /// </summary>
        [BindProperty]
        public PublicacionRequest Datos { get; set; } = new();


        /// <summary>
        /// Publicaicon en si
        /// </summary>
        public PublicacionResponse? Publicacion { get; set; }

        /// <summary>
        /// Lista de masctoas que se pueden elegir mientras esten disp en el refugio
        /// </summary>
        public List<MascotaResponse> Mascotas { get; set; } = new();

        /// <summary>
        /// Mascota con publicacion abierta, estan deshabilitadas
        /// </summary>
        public HashSet<Guid> Ocupadas { get; set; } = new();

        /// <summary>
        /// Solo para ver, usado en cerradas
        /// </summary>
        public bool SoloLectura => Publicacion?.Estado == EstadoPublicacion.Cerrada;

        /// <summary>
        /// Publicaion con postulaciones, bloqueada directamente
        /// </summary>
        public bool TipoBloqueado => Publicacion != null && Publicacion.CantidadPostulaciones > 0;

        /// <summary>
        /// Mensaje para reusar
        /// </summary>
        public string? AvisoReusar { get; set; }


        /// <summary>
        /// Da true si el id no es una publciacion del refugio
        /// </summary>
        public bool NoEncontrada { get; set; }

        /// <summary>
        /// Error mensaje
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Aviso mensaje
        /// </summary>

        [TempData]
        public string? Mensaje { get; set; }

        public async Task OnGetAsync()
        {
            await CargarMascotasAsync();

            //editar o ver
            if (Id != null)
            {
                var respuesta = await _api.GetAsync<PublicacionResponse>($"api/Publicacion/{Id}");
                if (!respuesta.Exito || respuesta.Datos == null)
                {
                    NoEncontrada = true;
                    return;
                }

                Publicacion = respuesta.Datos;
                CargarDesde(Publicacion);
                return;
            }

            //nueva, reusando una cerrada
            if (Reusar != null)
            {
                var original = await _api.GetAsync<PublicacionResponse>($"api/Publicacion/{Reusar}");
                if (original.Exito && original.Datos != null)
                {
                    CargarDesde(original.Datos);

                    //la misma mascota , no tiene que haber otra publi
                    var disponible = Mascotas.Any(m => m.Id == original.Datos.MascotaId) && !Ocupadas.Contains(original.Datos.MascotaId);
                    if (!disponible)
                    {
                        Datos.MascotaId = Guid.Empty;
                        AvisoReusar = $"{original.Datos.MascotaNombre} no se puede publicar ahora (ya no está en el refugio o tiene otra publicación abierta). Elegí otra mascota.";
                    }
                }
                return;
            }

            //nueva con la localidad del refugio (en caso de otro , poner en descripcion)
            var refugio = await _api.GetAsync<RefugioResponse>("api/Refugio");
            Datos.Ubicacion = refugio.Datos?.Localidad ?? string.Empty;
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await RecargarAsync();
                return Page();
            }

            if (Id == null)
            {
                var creada = await _api.PostAsync<PublicacionResponse>("api/Publicacion", Datos);

                if (!creada.Exito)
                {
                    Error = creada.Error;
                    await RecargarAsync();
                    return Page();
                }

                Mensaje = "Publicación creada: ya se ve en el listado público.";
                return RedirectToPage("/Refugio/Publicaciones");
            }

            var editada = await _api.PutAsync<PublicacionResponse>($"api/Publicacion/{Id}", Datos);

            if (!editada.Exito)
            {
                Error = editada.Error;
                await RecargarAsync();
                return Page();
            }

            Mensaje = "Cambios guardados.";
            return RedirectToPage(new { id = Id });
        }

        /// <summary>
        /// Cargo las mascotas que estan el refugio, filtro si tienen publicacion abierta
        /// </summary>
        /// <returns></returns>
        private async Task CargarMascotasAsync()
        {
            var mascotas = await _api.GetAsync<List<MascotaResponse>>("api/Mascota");
            Mascotas = (mascotas.Datos ?? new())
                .Where(m => m.Estado == EstadoMascota.EnRefugio)
                .OrderBy(m => m.Nombre)
                .ToList();

            var publicaciones = await _api.GetAsync<List<PublicacionResponse>>("api/Publicacion");
            Ocupadas = (publicaciones.Datos ?? new())
                .Where(p => p.Estado != EstadoPublicacion.Cerrada)
                .Select(p => p.MascotaId)
                .ToHashSet();
        }

        /// <summary>
        /// Recarga la pagina y las masctoas si hubo un error para poder mostrar de nuevo sin recargar la pagina
        /// </summary>
        private async Task RecargarAsync()
        {
            await CargarMascotasAsync();

            if (Id != null)
            {
                var respuesta = await _api.GetAsync<PublicacionResponse>($"api/Publicacion/{Id}");
                Publicacion = respuesta.Datos;
            }
        }

        /// <summary>
        /// Publicaicon en si para mostrar
        /// </summary>
        /// <param name="p"></param>
        private void CargarDesde(PublicacionResponse p)
        {
            Datos = new PublicacionRequest
            {
                MascotaId = p.MascotaId,
                Titulo = p.Titulo,
                Descripcion = p.Descripcion,
                Ubicacion = p.Ubicacion,
                Tipo = p.Tipo,
                PlazoEstimado = p.PlazoEstimado
            };
        }

    }
}
