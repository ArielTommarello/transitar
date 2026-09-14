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

            return postulacion == null ? null : PostulacionDTO(postulacion);
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
            if (publicacion == null)
                return Error("La publicacion no existe.");

            //publicacion no activa
            if (publicacion.Estado != EstadoPublicacion.Activa)
                return Error("La publicacion ya no esta recibiendo postulaciones o se encuentra pausada.");

            ////Postulacion unica por persona, sino se vuelve reptitivo, sirve como flag para mostras "estas postualdo"
            //var yaPostulado = await _context.Postulaciones
            //    .AnyAsync(p => p.PublicacionId == request.PublicacionId && p.UsuarioId == usuarioId);

            ////Flag para ya postulado
            //if (yaPostulado)
            //    return Error("Ya te postulaste a esta publicacion.");

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

            //una sola postulacion por persona y publicacion, salvo que se haya retirado
            var existente = await _context.Postulaciones
                .FirstOrDefaultAsync(p => p.PublicacionId == request.PublicacionId
                                       && p.UsuarioId == usuarioId);

            if (existente != null)
            {
                //sigue abierta o el refugio la rechazo: no puede volver a postularse
                if (existente.Estado != EstadoPostulacion.Retirada)
                    return Error("Ya te postulaste a esta publicacion.");

                //se habia retirado por decision propia: se reabre la misma fila
                existente.Estado = EstadoPostulacion.Pendiente;
                existente.FechaPostulacion = DateTime.UtcNow;
                existente.FechaResolucion = null;
                existente.ObservacionRechazo = null;
                existente.DisponibilidadFecha = request.DisponibilidadFecha;
                existente.DisponibilidadHorario = request.DisponibilidadHorario;

                await _context.SaveChangesAsync();

                return new PostulacionResult
                {
                    Postulacion = await ObtenerPostulacionAsync(existente.Id, usuarioId)
                };
            }



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
            if (postulacion == null)
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


        //Postulacion desde el lado del refugio que hizo la publicacion

        ///<inheritdoc/>
        public async Task<List<PostulacionRefugioResponse>> ListarPostulacionesDePublicacionAsync(Guid publicacionId, Guid refugioId)
        {
            //chequea que sea del refugio
            var esDelRefugio = await _context.Publicaciones
                .AnyAsync(p => p.Id == publicacionId && p.Mascota!.RefugioId == refugioId);

            if (!esDelRefugio)
                return new List<PostulacionRefugioResponse>();

            var postulaciones = await _context.Postulaciones
                .AsNoTracking()
                .Include(p => p.Usuario)!
                    .ThenInclude(u => u!.Perfil)!
                        .ThenInclude(perfil => perfil!.Contactos)
                .Where(p => p.PublicacionId == publicacionId)
                .OrderBy(p => p.FechaPostulacion)
                .ToListAsync();

            return postulaciones.Select(PostulacionRefugioDTO).ToList();
        }

        ///<inheritdoc/>
        public async Task<PostulacionResult> AceptarAsync(Guid publicacionId, Guid postulacionId, Guid refugioId)
        {
            var publicacion = await _context.Publicaciones
                .Include(p => p.Postulaciones)
                .FirstOrDefaultAsync(p => p.Id == publicacionId && p.Mascota!.RefugioId == refugioId);

            //si la publicacion esta vacia
            if (publicacion == null)
                return Error("No encontramos esa publicacion.");

            var postulacion = publicacion.Postulaciones.FirstOrDefault(p => p.Id == postulacionId);

            //si la postulacion no es de la publiacion o no existe
            if (postulacion == null)
                return Error("Esa postulacion no pertenece a esta publicacion.");

            //La postulacion tiene que estar pendiente, no aceptada o rechazada
            if (postulacion.Estado != EstadoPostulacion.Pendiente)
                return Error("Solo se pueden aceptar postulaciones pendientes.");

            //chequeo que sea el primero en se aceptado
            if (publicacion.Postulaciones.Any(p => p.Estado == EstadoPostulacion.Aceptada))
                return Error("Ya aceptaste a un candidato. Resolve esa postulacion antes de aceptar otra.");

            var fechaActual = DateTime.UtcNow;

            //actualizo la postulacion
            postulacion.Estado = EstadoPostulacion.Aceptada;
            postulacion.FechaResolucion = fechaActual;


            //pongo a las demas posutlaicones en espera hasta que resuelva con la aceptada
            foreach (var otra in publicacion.Postulaciones.Where(p => p.Id != postulacionId && p.Estado == EstadoPostulacion.Pendiente))
                    otra.Estado = EstadoPostulacion.EnEspera;
            //paso la publicacion en pausa
            publicacion.Estado = EstadoPublicacion.Pausada;

            await _context.SaveChangesAsync();

            return new PostulacionResult
            {
                Postulacion = await ObtenerPostulacionPorIdAsync(postulacionId)
            };
        }

        ///<inheritdoc/>
        public async Task<PostulacionResult> RechazarAsync(Guid publicacionId, Guid postulacionId, string? observacion, Guid refugioId)
        {
            
            var publicacion = await _context.Publicaciones
                .Include(p => p.Postulaciones)
                .FirstOrDefaultAsync(p => p.Id == publicacionId && p.Mascota!.RefugioId == refugioId);

            //si la publicacion esta vacia
            if (publicacion == null)
                return Error("No encontramos esa publicacion.");

            var postulacion = publicacion.Postulaciones.FirstOrDefault(p => p.Id == postulacionId);

            //reviso que la postulacion sea d mi publicacion
            if (postulacion == null)
                return Error("Esa postulacion no pertenece a esta publicacion.");

            //doble chequeo para ver que no este rechazada ni retirada
            if (postulacion.Estado == EstadoPostulacion.Rechazada
             || postulacion.Estado == EstadoPostulacion.Retirada)
                return Error("Esa postulacion ya estaba resuelta.");


            var Aceptada = postulacion.Estado == EstadoPostulacion.Aceptada;

            postulacion.Estado = EstadoPostulacion.Rechazada;
            postulacion.FechaResolucion = DateTime.UtcNow;
            postulacion.ObservacionRechazo = string.IsNullOrWhiteSpace(observacion)
                ? null
                : observacion.Trim();

            //si era el aceptado , se reabre la postualcion par aque ingresen nuevos candidatos y se revisa entre los que estaban
            if (Aceptada)
            {
                foreach (var otra in publicacion.Postulaciones.Where(p => p.Estado == EstadoPostulacion.EnEspera))
                    otra.Estado = EstadoPostulacion.Pendiente;

                publicacion.Estado = EstadoPublicacion.Activa;
            }

            await _context.SaveChangesAsync();

            return new PostulacionResult
            {
                Postulacion = await ObtenerPostulacionPorIdAsync(postulacionId)
            };
        }



        /// <summary>
        /// Consulta que se utiliza para todas cosntulas bases del DTO (para no repetir)
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
        /// Manejo del error y dice cual es
        /// </summary>
        private static PostulacionResult Error(string mensaje) => new() { Error = mensaje };



        /// <summary>
        /// Obtiene una postulacion sin filtrar por usuario. Lo usa el refugio, que llega por su publicacion y no por el dueño de la postulacion )la busqueda la realizo por la publicaicon
        /// </summary>
        private async Task<PostulacionResponse?> ObtenerPostulacionPorIdAsync(Guid id)
        {
            var postulacion = await ConsultaCompleta().FirstOrDefaultAsync(p => p.Id == id);
            return postulacion == null ? null : PostulacionDTO(postulacion);
        }



        /// <summary>
        /// Pasa la entidad al DTO de salida (postualcion que ve el usuario)
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
        /// Pasa la entidad al DTO de salida (postualcion que ve el refugio)
        /// </summary>
        private static PostulacionRefugioResponse PostulacionRefugioDTO(Postulacion p) => new()
        {
            Id = p.Id,
            Estado = p.Estado,
            DisponibilidadFecha = p.DisponibilidadFecha,
            DisponibilidadHorario = p.DisponibilidadHorario,
            FechaPostulacion = p.FechaPostulacion,
            FechaResolucion = p.FechaResolucion,

            UsuarioId = p.UsuarioId,
            Nombre = p.Usuario?.Nombre ?? string.Empty,
            Apellido = p.Usuario?.Apellido ?? string.Empty,

            FotoUrl = p.Usuario?.Perfil?.FotoUrl,
            Seleccion = p.Usuario?.Perfil?.Seleccion ?? default,
            TipoVivienda = p.Usuario?.Perfil?.TipoVivienda,
            TienePatio = p.Usuario?.Perfil?.TienePatio ?? false,
            PatioCerrado = p.Usuario?.Perfil?.PatioCerrado ?? false,
            TieneOtrasMascotas = p.Usuario?.Perfil?.TieneOtrasMascotas ?? false,
            DetalleOtrasMascotas = p.Usuario?.Perfil?.DetalleOtrasMascotas,
            HorasSoloPorDia = p.Usuario?.Perfil?.HorasSoloPorDia,
            CercaniaVeterinaria = p.Usuario?.Perfil?.CercaniaVeterinaria,
            ExperienciaPrevia = p.Usuario?.Perfil?.ExperienciaPrevia,
            MotivoPostulacion = p.Usuario?.Perfil?.MotivoPostulacion,
            FechaCompletado = p.Usuario?.Perfil?.FechaCompletado ?? default,

            Contactos = p.Usuario?.Perfil?.Contactos
                .Select(c => new ContactoResponse { Tipo = c.Tipo, Url = c.Url })
                .ToList() ?? new List<ContactoResponse>()
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
