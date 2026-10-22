using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using TransitAR.Structures;
using TransitAR.Web.Helpers;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Refugio
{
    public class MascotaModel : PageModel
    {

        private readonly ApiClient _api;

        public MascotaModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Id de la mascota (DTO)
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public Guid? Id { get; set; }

        /// <summary>
        /// Datos del dto de la mascota
        /// </summary>
        [BindProperty]
        public MascotaRequest Datos { get; set; } = new();

        /// <summary>
        /// Edad aprox en años (formato fecha)
        /// </summary>
        [BindProperty]
        [Range(0, 30, ErrorMessage = "Los años tienen que estar entre 0 y 30.")]
        public int? Anios { get; set; }

        /// <summary>
        /// Meses aprox por si no se peude mostrar años
        /// </summary>
        [BindProperty]
        [Range(0, 11, ErrorMessage = "Los meses tienen que estar entre 0 y 11.")]
        public int? Meses { get; set; }


        /// <summary>
        /// Dias aprox por si no se puede mostrar años o se es cachorro
        /// </summary>
        [BindProperty]
        [Range(0, 30, ErrorMessage = "Los días tienen que estar entre 0 y 30.")]
        public int? Dias { get; set; }

        /// <summary>
        /// La mascota en si para edicion
        /// </summary>
        public MascotaResponse? Mascota { get; set; }


        /// <summary>
        /// catalogo para usar en los deplegables al completar
        /// </summary>
        public List<EspecieResponse> Especies { get; set; } = new();
        public List<CondicionResponse> Condiciones { get; set; } = new();


        /// <summary>
        /// auxiliar, true si no esta el id de la mascota
        /// </summary>
        public bool NoEncontrada { get; set; }


        /// <summary>
        /// mensja  de la api
        /// </summary>
        [TempData]
        public string? Mensaje { get; set; }

        /// <summary>
        /// error de la api
        /// </summary>
        public string? Error { get; set; }

        public async Task OnGetAsync()
        {
            await CargarCatalogosAsync();

            //formulario nuevo
            if (Id == null)
                return;

            var respuesta = await _api.GetAsync<MascotaResponse>($"api/Mascota/{Id}");
            if (!respuesta.Exito || respuesta.Datos == null)
            {
                NoEncontrada = true;
                return;
            }

            Mascota = respuesta.Datos;
            CargarDesde(Mascota);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await RecargarAsync();
                return Page();
            }

            //cambio la fecha a años o meses apra poder mostrar (fecha que puso el refugio)
            Datos.FechaNacimientoAproximada = Formato.FechaDesdeEdad(Anios, Meses, Dias);

            if (Id == null)
            {
                var creada = await _api.PostAsync<MascotaResponse>("api/Mascota", Datos);

                //reviso que este creada
                if (!creada.Exito || creada.Datos == null)
                {
                    Error = creada.Error;
                    await RecargarAsync();
                    return Page();
                }

                //Para subir fotos tengo que tener creada la mascota, sino da error 
                Mensaje = "Mascota creada. Ahora podés subirle fotos.";
                return RedirectToPage(new { id = creada.Datos.Id });
            }

            var editada = await _api.PutAsync<MascotaResponse>($"api/Mascota/{Id}", Datos);

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
        /// Cargo los catalogos de especies y condiciones 
        /// </summary>
        /// <returns></returns>
        private async Task CargarCatalogosAsync()
        {
            var especies = await _api.GetAsync<List<EspecieResponse>>("api/Especie");
            Especies = especies.Datos ?? new();

            var condiciones = await _api.GetAsync<List<CondicionResponse>>("api/Condicion");
            Condiciones = condiciones.Datos ?? new();
        }

        /// <summary>
        /// Reload de pagina, para tomar catalogos o arreglo de errores (HCCORE)
        /// </summary>
        /// <returns></returns>
        private async Task RecargarAsync()
        {
            await CargarCatalogosAsync();

            if (Id != null)
            {
                var respuesta = await _api.GetAsync<MascotaResponse>($"api/Mascota/{Id}");
                Mascota = respuesta.Datos;
            }
        }


        /// <summary>
        /// Mascota para mostrar en el formulario
        /// </summary>
        /// <param name="m"></param>
        private void CargarDesde(MascotaResponse m)
        {
            Datos = new MascotaRequest
            {
                Nombre = m.Nombre,
                EspecieId = m.EspecieId,
                CondicionId = m.CondicionId,
                Raza = m.Raza,
                Sexo = m.Sexo,
                Tamanio = m.Tamanio,
                Vacunado = m.Vacunado
            };

            //edad en meses , dias , años
            if (m.FechaNacimientoAproximada != null)
            {
                var hoy = DateTime.Today;
                var nacimiento = m.FechaNacimientoAproximada.Value.Date;

                var anios = hoy.Year - nacimiento.Year;
                var meses = hoy.Month - nacimiento.Month;
                var dias = hoy.Day - nacimiento.Day;

                //si los dias dan negativo, agrego u nmes mas
                if (dias < 0)
                {
                    meses--;
                    var mesAnterior = hoy.AddMonths(-1);
                    dias += DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month);
                }

                //si me dan neg los meses, pido un año
                if (meses < 0)
                {
                    anios--;
                    meses += 12;
                }

                Anios = anios;
                Meses = meses;
                Dias = dias;
            }
        }


    }
}
