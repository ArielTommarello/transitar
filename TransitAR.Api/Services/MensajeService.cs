using Microsoft.EntityFrameworkCore;
using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    /// <summary>
    /// Implementacion del chat interno. Postualcion tiene la conversacion entre postulante y refugio.
    /// </summary>
    public class MensajeService : IMensajeService
    {

        private readonly TransitARContext _context;
        private readonly IConfiguration _configuration;



        /// <summary>
        /// Tipos de archivos permitidos en por seguridad para los adjuntos que iran en el chat.
        /// </summary>
        private static readonly string[] AdjuntosPermitidos = { "image/jpeg", "image/png", "image/webp", "video/mp4" };

        /// <summary>
        /// cantidad maximo de adjuntos por menseja. Limite 10Mb x 3  por el limite que pone .NET (se puede tocar en configuracion si es necesario)
        /// </summary>
        private const int MaxAdjuntos = 3;



        /// <summary>
        /// Inicializa el contexto y la config
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configuration"></param>
        public MensajeService(TransitARContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        ///<inheritdoc/>
        public async Task<List<ChatResumenResponse>> ListarChatsAsync(Guid usuarioId, Guid? refugioId, Guid? publicacionId, Guid? mascotaId)
        {
            if (usuarioId == Guid.Empty)
                return new List<ChatResumenResponse>();

            //consulta optimizada 
            var consulta = ConsultaCompleta().AsNoTracking();

            //busqueda desde los dos putnos (emisor-receptor)
            if (refugioId != null)
                consulta = consulta.Where(p => p.Publicacion!.Mascota!.RefugioId == refugioId.Value);
            else
                consulta = consulta.Where(p => p.UsuarioId == usuarioId);

            //busqueda desde una publicacion
            if (publicacionId != null)
                consulta = consulta.Where(p => p.PublicacionId == publicacionId.Value);

            //busuqeda por masctoa 
            if (mascotaId != null)
                consulta = consulta.Where(p => p.Publicacion!.MascotaId == mascotaId.Value);

            var postulaciones = await consulta.ToListAsync();

            //devuelvo postulaciones por orden
            return postulaciones
                .Select(p => ChatResumenDTO(p, usuarioId, refugioId != null))
                .OrderByDescending(c => c.FechaUltimoMensaje ?? DateTime.MinValue)
                .ToList();
        }

        ///<inheritdoc/>
        public async Task<ChatResponse?> ObtenerChatAsync(Guid postulacionId, Guid usuarioId, Guid? refugioId)
        {
            if (postulacionId == Guid.Empty || usuarioId == Guid.Empty)
                return null;

            //escribo fechas de lectura
            var postulacion = await ConsultaCompleta()
                .FirstOrDefaultAsync(p => p.Id == postulacionId);

            //error si no existe o no tiene acceso
            if (postulacion == null || !TieneAcceso(postulacion, usuarioId, refugioId))
                return null;

            //mantengo sin leer  los que no se abrieron
            var sinLeer = postulacion.Mensajes
                .Where(m => m.EmisorId != usuarioId && m.FechaLectura == null)
                .ToList();

            //reviso los que no se leyeron y los pongo leidos
            if (sinLeer.Count > 0)
            {
                var ahora = DateTime.UtcNow;

                foreach (var m in sinLeer)
                {
                    m.FechaLectura = ahora;
                }

                await _context.SaveChangesAsync();
            }

            return ChatDTO(postulacion, usuarioId, refugioId != null);
        }

        ///<inheritdoc/>
        public async Task<MensajeResult> EnviarAsync(Guid postulacionId, MensajeRequest request, List<ArchivoAdjunto> adjuntos, Guid usuarioId, Guid? refugioId)
        {
            if (request == null || postulacionId == Guid.Empty || usuarioId == Guid.Empty)
                return Error("No se recibieron los datos del mensaje.");

            var enviados = adjuntos ?? new List<ArchivoAdjunto>();

            var postulacion = await ConsultaCompleta()
                .FirstOrDefaultAsync(p => p.Id == postulacionId);

            //error si no existe o no tiene acceso
            if (postulacion == null || !TieneAcceso(postulacion, usuarioId, refugioId))
                return Error("No encontramos esa conversacion.");

            //reviso que no este en modosololectura
            if (!ChatAbierto(postulacion))
                return Error("Esta conversacion esta cerrada. Podes leerla pero ya no se pueden enviar mensajes.");

            //reviso que los adjuntos cumplan las condiciones
            var errorAdjuntos = ValidarAdjuntos(enviados);
            if (errorAdjuntos != null)
                return Error(errorAdjuntos);

            var mensaje = new Mensaje
            {
                Id = Guid.NewGuid(),
                PostulacionId = postulacion.Id,
                EmisorId = usuarioId,
                Texto = request.Texto?.Trim() ?? string.Empty,
                FechaEnvio = DateTime.UtcNow,
                FechaLectura = null
            };

            //adjuntos en el mensaje
            foreach (var archivo in enviados)
            {
                mensaje.Adjuntos.Add(new AdjuntoMensaje
                {
                    Id = Guid.NewGuid(),
                    MensajeId = mensaje.Id,
                    NombreArchivo = archivo.NombreArchivo.Trim(),
                    ContentType = archivo.ContentType.Trim().ToLowerInvariant(),
                    TamanioBytes = archivo.Contenido.LongLength,
                    Contenido = archivo.Contenido
                });
            }

            //guardo mensajae y adjuntos juntos
            _context.Mensajes.Add(mensaje);
            await _context.SaveChangesAsync();

            var emisor = await _context.Usuarios.FindAsync(usuarioId);

            return new MensajeResult
            {
                Mensaje = MensajeDTO(mensaje, usuarioId, NombreDe(emisor))
            };
        }

        ///<inheritdoc/>
        public async Task<ArchivoAdjunto?> ObtenerAdjuntoAsync(Guid adjuntoId, Guid usuarioId, Guid? refugioId)
        {
            if (adjuntoId == Guid.Empty || usuarioId == Guid.Empty)
                return null;
            //traigo los adjuntos del mensaje
            var adjunto = await _context.AdjuntosMensaje
                .AsNoTracking()
                .Include(a => a.Mensaje)!
                    .ThenInclude(m => m!.Postulacion)!
                        .ThenInclude(p => p!.Publicacion)!
                            .ThenInclude(pub => pub!.Mascota)
                .FirstOrDefaultAsync(a => a.Id == adjuntoId);

            if (adjunto?.Mensaje?.Postulacion == null)
                return null;

            //verifico que haya acceso para leer o usar esos adjuntos
            if (!TieneAcceso(adjunto.Mensaje.Postulacion, usuarioId, refugioId))
                return null;

            return new ArchivoAdjunto
            {
                NombreArchivo = adjunto.NombreArchivo,
                ContentType = adjunto.ContentType,
                Contenido = adjunto.Contenido
            };
        }

        /// <summary>
        /// Consulta base para el DTO de chat
        /// </summary>
        private IQueryable<Postulacion> ConsultaCompleta() =>
            _context.Postulaciones
                .Include(p => p.Usuario)
                .Include(p => p.Tenencia)
                .Include(p => p.Publicacion)!
                    .ThenInclude(pub => pub!.Mascota)!
                        .ThenInclude(m => m!.Refugio)
                .Include(p => p.Mensajes)
                    .ThenInclude(m => m.Emisor)
                .Include(p => p.Mensajes)
                    .ThenInclude(m => m.Adjuntos);

        /// <summary>
        /// Si quien consulta tiene acceso (una de las dos partes,refuio o postulante) El refugio lo tomo desde la mascota
        /// </summary>
        private static bool TieneAcceso(Postulacion p, Guid usuarioId, Guid? refugioId) =>
            refugioId != null
                ? p.Publicacion?.Mascota?.RefugioId == refugioId.Value
                : p.UsuarioId == usuarioId;

        /// <summary>
        /// Si todavia se puede escribir. Abierto hasta que termina la seleccion o tenencia por transito
        /// </summary>
        private static bool ChatAbierto(Postulacion p) =>
            p.Estado == EstadoPostulacion.Pendiente
            || p.Estado == EstadoPostulacion.EnEspera
            || (p.Estado == EstadoPostulacion.Aceptada
                && (p.Tenencia == null || p.Tenencia.FechaFinReal == null));

        /// <summary>
        /// Chequeo cantidad, tamaño y tipo de los archivos
        /// </summary>
        private string? ValidarAdjuntos(List<ArchivoAdjunto> adjuntos)
        {
            if (adjuntos.Count == 0)
                return null;

            //validacion de cantidad  para el problema con EFCORE
            if (adjuntos.Count > MaxAdjuntos)
                return $"Se pueden adjuntar hasta {MaxAdjuntos} archivos por mensaje.";

            //calculo bytes
            var maxMB = _configuration.GetValue<int?>("Reglas:AdjuntoMaxMB") ?? 10;
            var maxBytes = (long)maxMB * 1024 * 1024;

            //validacion por cada adjunto (para errores personalizados)
            foreach (var archivo in adjuntos)
            {
                if (archivo.Contenido.LongLength == 0)
                    return $"El archivo {archivo.NombreArchivo} esta vacio.";

                if (archivo.Contenido.LongLength > maxBytes)
                    return $"El archivo {archivo.NombreArchivo} supera los {maxMB} MB permitidos.";

                if (!AdjuntosPermitidos.Contains(archivo.ContentType.Trim().ToLowerInvariant()))
                    return "Solo se pueden adjuntar imagenes (jpg, png, webp) o videos mp4.";
            }

            return null;
        }

        /// <summary>
        /// Funcion auxiliar para obtener el Nombre y apellido de un usuario ( para mostrar en el chat )
        /// </summary>
        private static string NombreDe(Usuario? u) =>
            u == null ? string.Empty : $"{u.Nombre} {u.Apellido}".Trim();

        /// <summary>
        /// El nombre de la parte que recibe el mensaje (para usar en el front)
        /// </summary>
        private static string ReceptorDe(Postulacion p, bool consultaElRefugio) =>
            consultaElRefugio
                ? NombreDe(p.Usuario)
                : p.Publicacion?.Mascota?.Refugio?.Nombre ?? string.Empty;

        /// <summary>
        /// Resultado con error 
        /// </summary>
        private static MensajeResult Error(string mensaje) => new() { Error = mensaje };

        /// <summary>
        /// Pasa un mensaje al DTO. Nombre del emisor viene del que creo
        /// </summary>
        private static MensajeResponse MensajeDTO(Mensaje m, Guid usuarioId, string emisorNombre) => new()
        {
            Id = m.Id,
            Texto = m.Texto,
            FechaEnvio = m.FechaEnvio,
            FechaLectura = m.FechaLectura,
            EsPropio = m.EmisorId == usuarioId,
            EmisorNombre = emisorNombre,
            Adjuntos = m.Adjuntos
                .Select(a => new AdjuntoResponse
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    ContentType = a.ContentType,
                    TamanioBytes = a.TamanioBytes
                })
                .ToList()
        };

        /// <summary>
        /// Chat completo con la postulacion para usar en el DTO
        /// </summary>
        private static ChatResponse ChatDTO(Postulacion p, Guid usuarioId, bool consultaElRefugio) => new()
        {
            PostulacionId = p.Id,
            PublicacionId = p.PublicacionId,
            PublicacionTitulo = p.Publicacion?.Titulo ?? string.Empty,
            Tipo = p.Publicacion?.Tipo ?? default,
            MascotaId = p.Publicacion?.MascotaId ?? Guid.Empty,
            MascotaNombre = p.Publicacion?.Mascota?.Nombre ?? string.Empty,
            Receptor = ReceptorDe(p, consultaElRefugio),
            EstadoPostulacion = p.Estado,
            PuedeEscribir = ChatAbierto(p),
            Mensajes = p.Mensajes
                .OrderBy(m => m.FechaEnvio)
                .Select(m => MensajeDTO(m, usuarioId, NombreDe(m.Emisor)))
                .ToList()
        };

        /// <summary>
        /// Resumen o lista de chats con la postualcion para usar con el DTO
        /// </summary>
        private static ChatResumenResponse ChatResumenDTO(Postulacion p, Guid usuarioId, bool consultaElRefugio)
        {
            var ultimo = p.Mensajes.OrderByDescending(m => m.FechaEnvio).FirstOrDefault();

            return new ChatResumenResponse
            {
                PostulacionId = p.Id,
                PublicacionId = p.PublicacionId,
                MascotaId = p.Publicacion?.MascotaId ?? Guid.Empty,
                MascotaNombre = p.Publicacion?.Mascota?.Nombre ?? string.Empty,
                Tipo = p.Publicacion?.Tipo ?? default,
                Receptor = ReceptorDe(p, consultaElRefugio),
                EstadoPostulacion = p.Estado,
                PuedeEscribir = ChatAbierto(p),
                UltimoMensaje = ultimo?.Texto,
                FechaUltimoMensaje = ultimo?.FechaEnvio,
                UltimoTieneAdjunto = ultimo != null && ultimo.Adjuntos.Count > 0,
                NoLeidos = p.Mensajes.Count(m => m.EmisorId != usuarioId && m.FechaLectura == null)
            };
        }

    }
}
