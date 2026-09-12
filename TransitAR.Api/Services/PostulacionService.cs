using Microsoft.EntityFrameworkCore;
using TransitAR.Structures.Requests;
using TransitAR.Structures;

namespace TransitAR.Api.Services
{

    /// <summary>
    /// Implemebntacion de creacion y lectura de postulacuiones del postulante. Todas las consultras filtradas por el  usuario
    /// </summary>

    public class PostulacionService : IPostulacionService
    {

        private readonly TransitARContext _context;
        private readonly IConfiguration _configuration;

       /// <summary>
       /// Inicializa el contexto
       /// </summary>
       /// <param name="context"></param>
       /// <param name="configuration"></param>
        public PostulacionService(TransitARContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        ///<inheritdoc/>
        public async Task<List<PostulacionResponse>> ListarMisPostulacionesAsync(Guid usuarioId)
        {
            var postulaciones = await ConsultaCompleta()
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaPostulacion)
                .ToListAsync();

            return postulaciones.Select(PostulacionDTO).ToList();
        }

        ///<inheritdoc/>
        public async Task<PostulacionResponse?> ObtenerPostulacionAsync(Guid id, Guid usuarioId)
        {
            var postulacion = await ConsultaCompleta()
                .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);

            return postulacion is null ? null : PostulacionDTO(postulacion);
        }

        ///<inheritdoc/>
        public async Task<PostulacionResult> PostularseAsync(PostulacionRequest request, Guid usuarioId)
        {
            //uso de errores para diferenciar

            //error en caso de no tener datos o algo salio mal
            if (request == null || usuarioId == Guid.Empty)
                return Error("No se recibieron los datos de la postulacion, intentelo de nuevo mas tarde.");

            //usuario sin perfil completo
            if (!await _context.PerfilPostulantes.AnyAsync(p => p.UsuarioId == usuarioId))
                return Error("Tenes que completar tu perfil antes de poder postularte.");

            //postulacion creada y activa
            var publicacion = await _context.Publicaciones
                .FirstOrDefaultAsync(p => p.Id == request.PublicacionId);

            //publicaicon null
            if (publicacion is null)
                return Error("La publicacion no existe.");

            //publicacion no activa
            if (publicacion.Estado != EstadoPublicacion.Activa)
                return Error("La publicacion ya no esta recibiendo postulaciones o se encuentra pausada.");

            //Postulacion unica por persona, sino se vuelve reptitivo, sirve como flag para mostras "estas postualdo"
            var yaPostulado = await _context.Postulaciones
                .AnyAsync(p => p.PublicacionId == request.PublicacionId && p.UsuarioId == usuarioId);

            //Flag para ya postulado
            if (yaPostulado)
                return Error("Ya te postulaste a esta publicacion.");

            //herramienta pra en caso que el cupo de 0 o no funcione la cantidad puesta en appsettings
            var cupo = _configuration.GetValue<int?>("Reglas:CupoPostulacionesAbiertas") ?? 3;

            var abiertas = await _context.Postulaciones
                .CountAsync(p => p.UsuarioId == usuarioId
                              && (p.Estado == EstadoPostulacion.Pendiente
                               || p.Estado == EstadoPostulacion.EnEspera
                               || p.Estado == EstadoPostulacion.Aceptada));

            //Se cumplio la cantidad ed postulaciones que peude hacer el usuario
            if (abiertas >= cupo)
                return Error($"Ya tenes {cupo} postulaciones abiertas. Retira alguna para poder postularte a otra.");

            var postulacion = new Postulacion
            {
                Id = Guid.NewGuid(),
                PublicacionId = request.PublicacionId,
                UsuarioId = usuarioId,
                Estado = EstadoPostulacion.Pendiente,
                DisponibilidadFecha = request.DisponibilidadFecha,
                DisponibilidadHorario = request.DisponibilidadHorario,
                FechaPostulacion = DateTime.UtcNow
            };

            _context.Postulaciones.Add(postulacion);
            await _context.SaveChangesAsync();

            return new PostulacionResult
            {
                Postulacion = await ObtenerPostulacionAsync(postulacion.Id, usuarioId)
            };
        }

        ///<inheritdoc/>
        public async Task<PostulacionResult> RetirarPostulacionAsync(Guid id, Guid usuarioId)
        {
            var postulacion = await _context.Postulaciones
                .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);

            //error no existe la postulacon
            if (postulacion is null)
                return Error("No encontramos esa postulacion.");

            //solo se retira lo que todavia noe sta resuelto
            if (postulacion.Estado != EstadoPostulacion.Pendiente
             && postulacion.Estado != EstadoPostulacion.EnEspera)
                return Error("Solo se pueden retirar las postulaciones que siguen en evaluacion o no fueron retiradas.");

            postulacion.Estado = EstadoPostulacion.Retirada;
            postulacion.FechaResolucion = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new PostulacionResult
            {
                Postulacion = await ObtenerPostulacionAsync(id, usuarioId)
            };
        }

        /// <summary>
        /// Consulta que se utiliza para todas cosntulas bases del DTO (no repetir)
        /// </summary>
        private IQueryable<Postulacion> ConsultaCompleta() =>
            _context.Postulaciones
                .AsNoTracking()
                .Include(p => p.Publicacion)!
                    .ThenInclude(pub => pub!.Mascota)!
                        .ThenInclude(m => m!.Especie)
                .Include(p => p.Publicacion)!
                    .ThenInclude(pub => pub!.Mascota)!
                        .ThenInclude(m => m!.Refugio);

        /// <summary>
        /// Error fallido, y especifica cual es
        /// </summary>
        private static PostulacionResult Error(string mensaje) => new() { Error = mensaje };

        /// <summary>
        /// Pasa la entidad al DTO de salida
        /// </summary>
        private static PostulacionResponse PostulacionDTO(Postulacion p) => new()
        {
            Id = p.Id,
            Estado = p.Estado,
            DisponibilidadFecha = p.DisponibilidadFecha,
            DisponibilidadHorario = p.DisponibilidadHorario,
            FechaPostulacion = p.FechaPostulacion,
            FechaResolucion = p.FechaResolucion,
            MotivoRechazo = ResolverMotivoRechazo(p),

            PublicacionId = p.PublicacionId,
            PublicacionTitulo = p.Publicacion?.Titulo ?? string.Empty,
            Tipo = p.Publicacion?.Tipo ?? default,

            MascotaNombre = p.Publicacion?.Mascota?.Nombre ?? string.Empty,
            EspecieNombre = p.Publicacion?.Mascota?.Especie?.Nombre ?? string.Empty,
            FotosUrl = p.Publicacion?.Mascota?.FotosUrl,

            RefugioNombre = p.Publicacion?.Mascota?.Refugio?.Nombre ?? string.Empty
        };

        /// <summary>
        /// Devuelve el texto de rechazo que corresponde mostrar: el que escribio el refugio, su mensaje automatico o la leyenda por defecto del sistema
        /// </summary>
        private static string? ResolverMotivoRechazo(Postulacion p)
        {
            if (p.Estado != EstadoPostulacion.Rechazada)
                return null;

            if (!string.IsNullOrWhiteSpace(p.ObservacionRechazo))
                return p.ObservacionRechazo;

            var mensajeDelRefugio = p.Publicacion?.Mascota?.Refugio?.MensajeRechazoAutomatico;

            if (!string.IsNullOrWhiteSpace(mensajeDelRefugio))
                return mensajeDelRefugio;

            return "El refugio selecciono a otro candidato para esta publicacion.";
        }

    }
}
