using Microsoft.EntityFrameworkCore;
using TransitAR.Api.Extensions;
using TransitAR.Structures;


namespace TransitAR.Api.Services
{

    /// <summary>
    /// Implemebntacion de creacion y lectura de las publciaciones. La vista del refugio se filtra por el dueño a traves de la mascota.
    /// la vista publicaca se hace solo por el estado activo
    /// </summary>

    public class PublicacionService : IPublicacionService
    {

        private readonly TransitARContext _context;

        /// <summary>
        /// Inicializa el contexto
        /// </summary>
        /// <param name="context"></param>
        public PublicacionService(TransitARContext context)
        {
            _context = context;
        }

        ///<inheritdoc/>
        public async Task<List<PublicacionResponse>> ListarPublicacionesAsync(Guid refugioId)
        {
            var datos = await _context.Publicaciones
                .Include(p => p.Mascota)
                    .ThenInclude(m => m!.Fotos)
                .Where(p => p.Mascota!.RefugioId == refugioId)
                .OrderByDescending(p => p.FechaPublicacion)
                .Select(p => new { Publicacion = p, Cantidad = p.Postulaciones.Count })
                .ToListAsync();

            return datos.Select(d => PublicacionDTO(d.Publicacion, d.Cantidad)).ToList();
        }

        ///<inheritdoc/>
        public async Task<PublicacionResponse?> ObtenerPublicacionAsync(Guid id, Guid refugioId)
        {
            var dato = await _context.Publicaciones
                .Include(p => p.Mascota)
                     .ThenInclude (m => m!.Fotos)
                .Where(p => p.Id == id && p.Mascota!.RefugioId == refugioId)
                .Select(p => new { Publicacion = p, Cantidad = p.Postulaciones.Count })
                .FirstOrDefaultAsync();

            return dato == null ? null : PublicacionDTO(dato.Publicacion, dato.Cantidad);
        }


        ///<inheritdoc/>
        public async Task<PublicacionResult> CrearPublicacionAsync(PublicacionRequest request, Guid refugioId)
        {
            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m => m.Id == request.MascotaId && m.RefugioId == refugioId);

            if (mascota == null)
                return Error("No encontramos esa mascota en tu refugio.");

            //una en transito o adoptada ya tiene un apersona 
            if (mascota.Estado != EstadoMascota.EnRefugio)
                return Error("Solo se pueden publicar mascotas que estan en el refugio.");

            //una sola publicacion abierta (activa o pausada) por mascota , cerradas son historial
            var yaTieneAbierta = await _context.Publicaciones
                .AnyAsync(p => p.MascotaId == request.MascotaId && p.Estado != EstadoPublicacion.Cerrada);

            if (yaTieneAbierta)
                return Error("Esta mascota ya tiene una publicacion activa o pausada. Cerrala antes de crear otra.");

            var publicacion = new Publicacion
            {
                Id = Guid.NewGuid(),
                MascotaId = request.MascotaId,
                Titulo = request.Titulo.Trim(),
                Descripcion = request.Descripcion.Trim(),
                Ubicacion = request.Ubicacion.Trim(),
                Tipo = request.Tipo,
                Estado = EstadoPublicacion.Activa,
                //el plazo solo existe en un transito
                PlazoEstimado = request.Tipo == TipoPublicacion.Transito ? request.PlazoEstimado : null,
                FechaPublicacion = DateTime.UtcNow
            };

            _context.Publicaciones.Add(publicacion);
            await _context.SaveChangesAsync();

            return new PublicacionResult { Publicacion = await ObtenerPublicacionAsync(publicacion.Id, refugioId) };
        }

        ///<inheritdoc/>
        public async Task<PublicacionResult> ActualizarPublicacionAsync(Guid id, PublicacionRequest request, Guid refugioId)
        {
          
            var publicacion = await _context.Publicaciones
                .FirstOrDefaultAsync(p => p.Id == id && p.Mascota!.RefugioId == refugioId);

            if (publicacion == null)
                return Error("No encontramos esa publicacion en tu refugio.");

            //cerrada (archivada)
            if (publicacion.Estado == EstadoPublicacion.Cerrada)
                return Error("Una publicacion cerrada no se edita.");

            //no se peude cambiar si hay gente psotulada
            if (request.Tipo != publicacion.Tipo
             && await _context.Postulaciones.AnyAsync(p => p.PublicacionId == id))
                return Error("No se puede cambiar entre adopcion y transito porque la publicacion ya tiene postulaciones.");

            
            publicacion.Titulo = request.Titulo.Trim();
            publicacion.Descripcion = request.Descripcion.Trim();
            publicacion.Ubicacion = request.Ubicacion.Trim();
            publicacion.Tipo = request.Tipo;
            publicacion.PlazoEstimado = request.Tipo == TipoPublicacion.Transito ? request.PlazoEstimado : null;

            await _context.SaveChangesAsync();

            return new PublicacionResult { Publicacion = await ObtenerPublicacionAsync(id, refugioId) };
        }


        ///<inheritdoc/>
        public async Task<PublicacionResult> CambiarEstadoAsync(Guid id, EstadoPublicacion estado, Guid refugioId)
        {
            var publicacion = await _context.Publicaciones
                 .Include(p => p.Postulaciones)
                     .ThenInclude(po => po.Tenencia)
                 .FirstOrDefaultAsync(p => p.Id == id && p.Mascota!.RefugioId == refugioId);


            if (publicacion == null)
                return Error("No encontramos esa publicacion en tu refugio.");

            //cerrada (archivada) , crear nueva para la mascota
            if (publicacion.Estado == EstadoPublicacion.Cerrada)
                return Error("Una publicacion cerrada no se puede reabrir. Si la mascota sigue disponible, crea una publicacion nueva.");

            var ahora = DateTime.UtcNow;

            publicacion.Estado = estado;

            //al cerrar se marca la fecha; si se vuelve a abrir se vuelve a cambiar
            publicacion.FechaCierre = estado == EstadoPublicacion.Cerrada
                ? ahora
                : null;

            //cierre a mano para que las postulaciones que estaban abiertas se le cierran al postulante.
            if (estado == EstadoPublicacion.Cerrada)
            {
                foreach (var abierta in publicacion.Postulaciones.Where(p =>
                             p.Estado == EstadoPostulacion.Pendiente
                          || p.Estado == EstadoPostulacion.EnEspera
                          || (p.Estado == EstadoPostulacion.Aceptada && p.Tenencia == null)))
                {
                    abierta.Estado = EstadoPostulacion.Rechazada;
                    abierta.FechaResolucion = ahora;
                    abierta.ObservacionRechazo = MotivoCierre;
                }
            }


            await _context.SaveChangesAsync();

            return new PublicacionResult { Publicacion = await ObtenerPublicacionAsync(id, refugioId) };
        }


        ///<inheritdoc/>
        public async Task<PublicacionPublicaResponse?> ObtenerActivaAsync(Guid id)
        {
            var publicacion = await _context.Publicaciones
                .AsNoTracking()
                .Include(p => p.Mascota)!.ThenInclude(m => m!.Especie)
                .Include(p => p.Mascota)!.ThenInclude(m => m!.Refugio)
                .Include(p => p.Mascota)!.ThenInclude(m => m!.Fotos)
                .FirstOrDefaultAsync(p => p.Id == id && p.Estado == EstadoPublicacion.Activa && p.Mascota!.Refugio!.Activo);

            return publicacion == null ? null : PublicacionPublicaDTO(publicacion);
        }


        //VISTA PUBLICA - para la visual del usuario

        ///<inheritdoc/>
        public async Task<List<PublicacionPublicaResponse>> ListarActivasAsync(TipoPublicacion? tipo,Guid? especieId,Tamanio? tamanio,Sexo? sexo,string? ubicacion)
        {
            //primer consulta,todas las publicaciones
            var consulta = _context.Publicaciones
                .AsNoTracking()
                .Include(p => p.Mascota)!.ThenInclude(m => m!.Especie)
                .Include(p => p.Mascota)!.ThenInclude(m => m!.Refugio)
                .Include(p => p.Mascota)!.ThenInclude(m => m!.Fotos)
                .Where(p => p.Estado == EstadoPublicacion.Activa && p.Mascota!.Refugio!.Activo);

            //filtro de tipo
            if (tipo != null)
                consulta = consulta.Where(p => p.Tipo == tipo.Value);

            //filtro de especie
            if (especieId != null)
                consulta = consulta.Where(p => p.Mascota!.EspecieId == especieId.Value);

            //filtro de tamanio
            if (tamanio != null)
                consulta = consulta.Where(p => p.Mascota!.Tamanio == tamanio.Value);

            //filtro de sexo
            if (sexo != null)
                consulta = consulta.Where(p => p.Mascota!.Sexo == sexo.Value);

            //filtrar por ubicaicon
            if (!string.IsNullOrWhiteSpace(ubicacion))
                consulta = consulta.Where(p => p.Ubicacion.Contains(ubicacion.Trim()));

            var publicaciones = await consulta
                .OrderByDescending(p => p.FechaPublicacion)
                .ToListAsync();

            return publicaciones.Select(PublicacionPublicaDTO).ToList();
        }


        /// <summary>
        /// Pasa la entidad al DTO que ve el refugio
        /// </summary>
        private static PublicacionResponse PublicacionDTO(Publicacion p, int cantidadPostulaciones) => new()
        {
            Id = p.Id,
            MascotaId = p.MascotaId,
            MascotaNombre = p.Mascota?.Nombre ?? string.Empty,
            FotosIds = MascotaService.IdsDeFotos(p.Mascota),
            Titulo = p.Titulo,
            Descripcion = p.Descripcion,
            Ubicacion = p.Ubicacion,
            Tipo = p.Tipo,
            Estado = p.Estado,            
            PlazoEstimado = p.PlazoEstimado,
            FechaPublicacion = p.FechaPublicacion,
            FechaCierre = p.FechaCierre,
            CantidadPostulaciones = cantidadPostulaciones
        };

        /// <summary>
        /// Pasa la entidad al DTO publico, con los datos de la mascota y del refugio 
        /// </summary>
        private static PublicacionPublicaResponse PublicacionPublicaDTO(Publicacion p) => new()
        {
            Id = p.Id,
            Titulo = p.Titulo,
            Descripcion = p.Descripcion,
            Ubicacion = p.Ubicacion,
            Tipo = p.Tipo,           
            PlazoEstimado = p.PlazoEstimado,
            FechaPublicacion = p.FechaPublicacion,

            MascotaNombre = p.Mascota?.Nombre ?? string.Empty,
            EspecieNombre = p.Mascota?.Especie?.Nombre ?? string.Empty,
            Sexo = p.Mascota?.Sexo,
            Tamanio = p.Mascota?.Tamanio,
            EdadAproximadaMeses = p.Mascota?.FechaNacimientoAproximada.EdadEnMeses(),
            EdadAproximadaDias = p.Mascota?.FechaNacimientoAproximada.EdadEnDias(),
            Vacunado = p.Mascota?.Vacunado ?? false,
            FotosIds = MascotaService.IdsDeFotos(p.Mascota),

            RefugioNombre = p.Mascota?.Refugio?.Nombre ?? string.Empty,
            RefugioLocalidad = p.Mascota?.Refugio?.Localidad
        };


        /// <summary>
        /// Motivo para el postulante si el refugio cierra la publicacion
        /// </summary>
        private const string MotivoCierre = "El refugio cerro la publicacion.";


        /// <summary>
        /// Resultado con el motivo por el que no se pudo
        /// </summary>
        private static PublicacionResult Error(string mensaje) => new() { Error = mensaje };


    }
}
