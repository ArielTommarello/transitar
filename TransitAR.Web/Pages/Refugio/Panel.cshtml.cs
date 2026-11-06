using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TransitAR.Structures.Requests;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Refugio
{
    /// <summary>
    /// Dashboard para el refugio: numeros importantes a atender, hsitorias y tenencias
    /// </summary>
    public class PanelModel : PageModel
    {

        private readonly ApiClient _api;

        public PanelModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Pedidos para las tarjetas, de cada cosa que maneja el refugio
        /// </summary>
        public List<MascotaResponse>? Mascotas { get; set; }
        public List<PublicacionResponse>? Publicaciones { get; set; }
        public List<TenenciaResponse>? Tenencias { get; set; }
        public List<SeguimientoResponse>? Controles { get; set; }
        public List<ChatResumenResponse>? Chats { get; set; }


        /// <summary>
        /// Listas con pendientes: uno por uno por cada postulacion + estado
        /// </summary>
        public int? PorRevisar => Chats?.Count(c => c.EstadoPostulacion == EstadoPostulacion.Pendiente);
        public int? ControlesAtrasados => Controles?.Count(c => c.Vencido);
        public int? TenenciasVencidas => ContarTenencias(VistaTenencias.Vencidas);
        public int? SinControles => ContarTenencias(VistaTenencias.SinControlesPendientes);
        public int? SinLeer => Chats?.Sum(c => c.NoLeidos);



        //hoy
        public int? EnRefugio => Mascotas?.Count(m => m.Estado == EstadoMascota.EnRefugio);
        public int? Activas => Publicaciones?.Count(p => p.Estado == EstadoPublicacion.Activa);
        public int? Pausadas => Publicaciones?.Count(p => p.Estado == EstadoPublicacion.Pausada);
        public int? EnCurso => ContarTenencias(VistaTenencias.EnCurso);


        //historial - tenencias
        public int? Adoptadas => Mascotas?.Count(m => m.Estado == EstadoMascota.Adoptada);
        public int? Finalizadas => ContarTenencias(VistaTenencias.Finalizadas);


        /// <summary>
        /// Controles qyue puede haber hoy y mañana para poner aviso arriba en el dashboarda )mismo uso en seguimiento)
        /// </summary>
        public List<SeguimientoResponse> ProximosDias =>
            Controles?.Where(c => !c.Vencido && SeguimientoModel.EsHoy(c)).ToList() ?? new();

        /// <summary>
        /// Contamos como en tnenecias para un mismo numero
        /// </summary>
        /// <param name="vista"></param>
        /// <returns></returns>
        private int? ContarTenencias(VistaTenencias vista) =>
            Tenencias?.Count(t => TenenciasModel.EnVista(t, vista));

        public async Task OnGetAsync()
        {
            Mascotas = (await _api.GetAsync<List<MascotaResponse>>("api/Mascota")).Datos;
            Publicaciones = (await _api.GetAsync<List<PublicacionResponse>>("api/Publicacion")).Datos;
            Tenencias = (await _api.GetAsync<List<TenenciaResponse>>("api/Tenencia")).Datos;
            Controles = (await _api.GetAsync<List<SeguimientoResponse>>($"api/Seguimiento?estado={(int)EstadoSeguimiento.Pendiente}")).Datos;
            Chats = (await _api.GetAsync<List<ChatResumenResponse>>("api/Mensaje/chats")).Datos;
        }
    }
}
