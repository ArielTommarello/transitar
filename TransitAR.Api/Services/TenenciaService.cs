using Microsoft.EntityFrameworkCore;
using TransitAR.Structures;
using TransitAR.Structures.Requests;

namespace TransitAR.Api.Services
{
    /// <summary>
    /// Implementaicon de la tenencias. Es por mascota que cada una pertenece al refugio
    /// </summary>
    public class TenenciaService : ITenenciaService
    {
        private readonly TransitARContext _context;

        /// <summary>
        /// Inicializa el contexto
        /// </summary>
        /// <param name="context"></param>
        public TenenciaService(TransitARContext context)
        {
            _context = context;
        }


        ///<inheritdoc/>
        public async Task<List<TenenciaResponse>> ListarTenenciasAsync(Guid refugioId)
        {
            var tenencias = await ConsultaCompleta()
                .Where(t => t.Mascota!.RefugioId == refugioId)
                .OrderByDescending(t => t.FechaInicio)
                .ToListAsync();

            return tenencias.Select(TenenciaDTO).ToList();
        }

        ///<inheritdoc/>
        public async Task<TenenciaResponse?> ObtenerTenenciaAsync(Guid id, Guid refugioId)
        {
            var tenencia = await ConsultaCompleta()
                .FirstOrDefaultAsync(t => t.Id == id && t.Mascota!.RefugioId == refugioId);

            return tenencia == null ? null : TenenciaDTO(tenencia);
        }

        ///<inheritdoc/>
        public async Task<TenenciaResult> ConfirmarEntregaAsync(TenenciaRequest request, Guid refugioId)
        {
            if (request == null || refugioId == Guid.Empty)
                return Error("No se recibieron los datos de la entrega.");

            //la postulacion con la publicaion a la que pertenece, la mascota y las demas postulaciones
            var postulacion = await _context.Postulaciones
                .Include(p => p.Publicacion)!
                    .ThenInclude(pub => pub!.Mascota)
                .Include(p => p.Publicacion)!
                    .ThenInclude(pub => pub!.Postulaciones)
                .FirstOrDefaultAsync(p => p.Id == request.PostulacionId && p.Publicacion!.Mascota!.RefugioId == refugioId);

            //no esta la publciacion
            if (postulacion == null)
                return Error("No encontramos esa postulacion en tus publicaciones.");

            //Confirmamos que la postulacion debe estar aceptada
            if (postulacion.Estado != EstadoPostulacion.Aceptada)
                return Error("Solo se puede confirmar la entrega de una postulacion aceptada.");

            //Postulacion que debe ser unica
            if (await _context.Tenencias.AnyAsync(t => t.PostulacionId == request.PostulacionId))
                return Error("Esta entrega ya fue confirmada.");

            var publicacion = postulacion.Publicacion!;
            var mascota = publicacion.Mascota!;
            var modalidad = publicacion.Tipo;
            var ahora = DateTime.UtcNow;

            //fecha estiamda en caso de transito, a futuro
            DateTime? fechaFinEstimada = null;

            if (modalidad == TipoPublicacion.Transito && request.FechaFinEstimada != null)
            {
                
                if (request.FechaFinEstimada <= ahora)
                    return Error("La fecha estimada de devolucion tiene que ser posterior a hoy.");

                fechaFinEstimada = request.FechaFinEstimada;
            }

            //auxiliar teenencia
            var tenencia = new Tenencia
            {
                Id = Guid.NewGuid(),
                PostulacionId = postulacion.Id,
                MascotaId = mascota.Id,
                Modalidad = modalidad,
                FechaInicio = ahora,
                FechaFinEstimada = fechaFinEstimada
            };

            _context.Tenencias.Add(tenencia);

            //cierre de publciaicon y rechazo (no de mala manera de los otros posutalntes)
            publicacion.Estado = EstadoPublicacion.Cerrada;
            publicacion.FechaCierre = ahora;

            
            foreach (var otra in publicacion.Postulaciones.Where(p => p.Id != postulacion.Id && (p.Estado == EstadoPostulacion.EnEspera || p.Estado == EstadoPostulacion.Pendiente)))
            {
                otra.Estado = EstadoPostulacion.Rechazada;
                otra.FechaResolucion = ahora;
                otra.ObservacionRechazo = null;
            }

            //Cambio de estado de la masctoa si es adoptada o transitada
            mascota.Estado = modalidad == TipoPublicacion.Adopcion ? EstadoMascota.Adoptada : EstadoMascota.EnTransito;

            await _context.SaveChangesAsync();

            //devuelvo la tenencia creada
            return new TenenciaResult
            {
                Tenencia = await ObtenerTenenciaAsync(tenencia.Id, refugioId)
            };
        }

        /// <summary>
        /// Consulta base con las navegaciones que necesita el DTO
        /// </summary>
        private IQueryable<Tenencia> ConsultaCompleta() =>
            _context.Tenencias
                .AsNoTracking()
                .Include(t => t.Mascota)
                .Include(t => t.Postulacion)!
                    .ThenInclude(p => p!.Usuario);

        /// <summary>
        /// Resultado error, en caso de no completarse bien la tenencia
        /// </summary>
        private static TenenciaResult Error(string mensaje) => new() { Error = mensaje };

        /// <summary>
        /// Pasa la entidad al DTO de salida, calculando si el transito esta vencido
        /// </summary>
        private static TenenciaResponse TenenciaDTO(Tenencia t) => new()
        {
            Id = t.Id,
            PostulacionId = t.PostulacionId,
            Modalidad = t.Modalidad,

            MascotaId = t.MascotaId,
            MascotaNombre = t.Mascota?.Nombre ?? string.Empty,

            UsuarioId = t.Postulacion?.UsuarioId ?? Guid.Empty,
            Nombre = t.Postulacion?.Usuario?.Nombre ?? string.Empty,
            Apellido = t.Postulacion?.Usuario?.Apellido ?? string.Empty,

            FechaInicio = t.FechaInicio,
            FechaFinEstimada = t.FechaFinEstimada,
            FechaFinReal = t.FechaFinReal,
            FechaConversion = t.FechaConversion,

            ObservacionCierre = t.ObservacionCierre,
            FinalizoBien = t.FinalizoBien,

            EstaVencida = t.Modalidad == TipoPublicacion.Transito && t.FechaFinReal == null && t.FechaFinEstimada != null && t.FechaFinEstimada < DateTime.UtcNow
        };


    }
}
