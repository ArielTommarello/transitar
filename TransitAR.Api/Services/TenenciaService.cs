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


        //para refugio (agenda tenencias)

        ///<inheritdoc/>
        public async Task<List<TenenciaResponse>> ListarTenenciasAsync(Guid refugioId, TipoPublicacion? modalidad, bool? enCurso, bool vencidas)
        {
            var ahora = DateTime.UtcNow;

            var consulta = ConsultaCompleta()
                .Where(t => t.Mascota!.RefugioId == refugioId);

            //filtro por modalidad (para no tener problema en caso de adopcion)
            if (modalidad != null)
                consulta = consulta.Where(t => t.Modalidad == modalidad.Value);

            //filtro para cerradas o en transito
            if (enCurso != null)
                consulta = enCurso.Value ? consulta.Where(t => t.FechaFinReal == null) : consulta.Where(t => t.FechaFinReal != null);

            //repito condicion del DTO para vencidas (EstaVencida).
            if (vencidas)
                consulta = consulta.Where(t => t.Modalidad == TipoPublicacion.Transito && t.FechaFinReal == null && t.FechaFinEstimada != null && t.FechaFinEstimada.Value.Date < ahora.Date);

            var tenencias = await consulta
                .OrderByDescending(t => t.FechaInicio)
                .ToListAsync();

            return tenencias.Select(TenenciaDTO).ToList();
        }


        //uso en devoluciones

        ///<inheritdoc/>
        public async Task<TenenciaResult> DevolverAsync(Guid id, DevolucionRequest request, Guid refugioId)
        {
            //no existe
            if (request == null)
                return Error("No se recibieron los datos de la devolucion.");

            var tenencia = await _context.Tenencias
                .Include(t => t.Mascota)
                .FirstOrDefaultAsync(t => t.Id == id && t.Mascota!.RefugioId == refugioId);

            //no existe la tenecia (nuca lo aceptamos()
            if (tenencia == null)
                return Error("No encontramos esa tenencia.");

            //problemas generales con la tenencia
            if (tenencia.FechaFinReal != null)
                return Error("Esta tenencia ya estaba cerrada.");

            var ahora = DateTime.UtcNow;

            tenencia.FechaFinReal = ahora;
            tenencia.FinalizoBien = request.FinalizoBien;
            tenencia.ObservacionCierre = string.IsNullOrWhiteSpace(request.ObservacionCierre)
                ? null : request.ObservacionCierre.Trim();

            //el animal vuelve a estar disponible: el refugio puede publicarlo de nuevo (nueva publicaion)
            tenencia.Mascota!.Estado = EstadoMascota.EnRefugio;

            await _context.SaveChangesAsync();

            return new TenenciaResult
            {
                Tenencia = await ObtenerTenenciaAsync(id, refugioId)
            };
        }


        ///<inheritdoc/>
        public async Task<TenenciaResult> ConvertirAAdopcionAsync(Guid id, Guid refugioId)
        {
            var tenencia = await _context.Tenencias
                .Include(t => t.Mascota)
                .FirstOrDefaultAsync(t => t.Id == id && t.Mascota!.RefugioId == refugioId);

            //no existe la tenencia
            if (tenencia == null)
                return Error("No encontramos esa tenencia.");


            //problemas varios tenecia
            if (tenencia.FechaFinReal != null)
                return Error("Esta tenencia ya estaba cerrada.");

            //flag para mostrar que solo sea de transsito a adopcion , no se peude de adopcion a transito o adopcion a adopcion
            if (tenencia.Modalidad != TipoPublicacion.Transito)
                return Error("Solo se puede convertir un transito: esta tenencia ya es una adopcion.");

            //trabajamos sobre misma tenencia, esta en el mismo lado

            tenencia.Modalidad = TipoPublicacion.Adopcion;
            tenencia.FechaConversion = DateTime.UtcNow;
            tenencia.FechaFinEstimada = null;

            //pasa a adoptada
            tenencia.Mascota!.Estado = EstadoMascota.Adoptada;

            await _context.SaveChangesAsync();

            return new TenenciaResult
            {
                Tenencia = await ObtenerTenenciaAsync(id, refugioId)
            };
        }

        //HISTORIAL DE TENENCIAS (USO REFUGIOS)

        ///<inheritdoc/>
        public async Task<List<HistorialTenenciaResponse>?> ObtenerHistorialPostulanteAsync(Guid usuarioId, Guid refugioId)
        {
            if (usuarioId == Guid.Empty || refugioId == Guid.Empty)
                return null;

            //solo se ve el historial de alguien que se postulo alguna vez a una publicacion de este refugio
            var sePostulo = await _context.Postulaciones
                .AnyAsync(p => p.UsuarioId == usuarioId
                            && p.Publicacion!.Mascota!.RefugioId == refugioId);

            if (!sePostulo)
                return null;

            //unificacion para usar tenencias con mascota

            return await TenenciasAsync(usuarioId);
        }


        //HISTORIAL DE TENENCIAS (USO USUARIOS)     

        ///<inheritdoc/>
        public async Task<List<HistorialTenenciaResponse>> ObtenerMisTenenciasAsync(Guid usuarioId)
        {
            if (usuarioId == Guid.Empty)
                return new List<HistorialTenenciaResponse>();

            return await TenenciasAsync(usuarioId);
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
        /// Un transito en curso con la fecha pactada ya pasada lo usan los dos DTO (listarTenenciaAsync no se toca, usa sql yef opara escribir)
        /// </summary>
        private static bool EsVencida(Tenencia t, DateTime ahora) =>
            t.Modalidad == TipoPublicacion.Transito
            && t.FechaFinReal == null
            && t.FechaFinEstimada != null
            && t.FechaFinEstimada.Value.Date < ahora.Date;

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

            EstaVencida = EsVencida(t, DateTime.UtcNow)
        };


        //DTO PARA HISTORIAL DE TENENCIAS
        /// <summary>
        /// Pasa la entidad al DTO que ve un refugio evaluando a un candidato
        /// </summary>
        private static HistorialTenenciaResponse HistorialDTO(Tenencia t)
        {
            var ahora = DateTime.UtcNow;
            var hasta = t.FechaFinReal ?? ahora;

            return new()
            {
                Id = t.Id,
                MascotaNombre = t.Mascota?.Nombre ?? string.Empty,
                RefugioNombre = t.Mascota?.Refugio?.Nombre ?? string.Empty,
                Modalidad = t.Modalidad,
                FechaInicio = t.FechaInicio,
                FechaFinEstimada = t.FechaFinEstimada,
                FechaFinReal = t.FechaFinReal,
                FechaConversion = t.FechaConversion,
                FinalizoBien = t.FinalizoBien,
                ObservacionCierre = t.ObservacionCierre,
                EnCurso = t.FechaFinReal == null,
                VencidaEnCurso = EsVencida(t, ahora),
                DuracionEnDias = (int)(hasta.Date - t.FechaInicio.Date).TotalDays
            };
        }

        /// <summary>
        /// Las tenencias de un postulante, sin control de acceso , remplaza al DTO en la busqueda esta para tener mascota.
        /// </summary> 
        private async Task<List<HistorialTenenciaResponse>> TenenciasAsync(Guid usuarioId)
        {
            var tenencias = await _context.Tenencias
                .AsNoTracking()
                .Include(t => t.Mascota)!
                    .ThenInclude(m => m!.Refugio)
                .Include(t => t.Postulacion)
                .Where(t => t.Postulacion!.UsuarioId == usuarioId)
                .OrderByDescending(t => t.FechaInicio)
                .ToListAsync();

            return tenencias.Select(HistorialDTO).ToList();
        }


    }
}
