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
                .Where(p => p.Id == id && p.Mascota!.RefugioId == refugioId)
                .Select(p => new { Publicacion = p, Cantidad = p.Postulaciones.Count })
                .FirstOrDefaultAsync();

            return dato is null ? null : PublicacionDTO(dato.Publicacion, dato.Cantidad);
        }


        ///<inheritdoc/>
        public async Task<PublicacionResponse?> CrearPublicacionAsync(PublicacionRequest request, Guid refugioId)
        {
            if (request == null)
                return null;

            if (refugioId == Guid.Empty)
                return null;

            //el tipo tiene que ser adopcion o transito, nunca 0 
            if (!Enum.IsDefined(request.Tipo))
                return null;

            //la mascota tiene que existir y ser del refugio autenticado
            var mascotaPropia = await _context.Mascotas
                .AnyAsync(m => m.Id == request.MascotaId && m.RefugioId == refugioId);

            if (!mascotaPropia)
                return null;

            //no puede haber dos publicaciones abiertas sobre la misma mascota
            var yaTieneAbierta = await _context.Publicaciones
                .AnyAsync(p => p.MascotaId == request.MascotaId
                            && p.Estado != EstadoPublicacion.Cerrada);

            if (yaTieneAbierta)
                return null;

            var publicacion = new Publicacion
            {
                Id = Guid.NewGuid(),
                MascotaId = request.MascotaId,
                Titulo = request.Titulo.Trim(),
                Descripcion = request.Descripcion.Trim(),
                Ubicacion = request.Ubicacion.Trim(),
                Tipo = request.Tipo,
                Estado = EstadoPublicacion.Activa,
                FotosUrlExtra = request.FotosUrlExtra,
                PlazoEstimado = request.PlazoEstimado,
                FechaPublicacion = DateTime.UtcNow
            };

            _context.Publicaciones.Add(publicacion);
            await _context.SaveChangesAsync();

            return await ObtenerPublicacionAsync(publicacion.Id, refugioId);
        }

        ///<inheritdoc/>
        public async Task<PublicacionResponse?> ActualizarPublicacionAsync(Guid id, PublicacionRequest request, Guid refugioId)
        {
            if (request == null)
                return null;

            //el tipo tiene que ser adopcion o transito, nunca 0 
            if (!Enum.IsDefined(request.Tipo))
                return null;

            var publicacion = await _context.Publicaciones
                .FirstOrDefaultAsync(p => p.Id == id && p.Mascota!.RefugioId == refugioId);

            if (publicacion is null)
                return null;

            //MascotaId no se toca cambiar de mascota seria otra publicacion o otra mascota (cada mascota solo tiene una publicacion)
            publicacion.Titulo = request.Titulo.Trim();
            publicacion.Descripcion = request.Descripcion.Trim();
            publicacion.Ubicacion = request.Ubicacion.Trim();
            publicacion.Tipo = request.Tipo;
            publicacion.FotosUrlExtra = request.FotosUrlExtra;
            publicacion.PlazoEstimado = request.PlazoEstimado;

            await _context.SaveChangesAsync();

            return await ObtenerPublicacionAsync(id, refugioId);
        }


        ///<inheritdoc/>
        public async Task<PublicacionResponse?> CambiarEstadoAsync(Guid id, EstadoPublicacion estado, Guid refugioId)
        {
            var publicacion = await _context.Publicaciones
                .FirstOrDefaultAsync(p => p.Id == id && p.Mascota!.RefugioId == refugioId);

            if (publicacion is null)
                return null;

            publicacion.Estado = estado;

            //al cerrar se marca la fecha; si se vuelve a abrir se vuelve a cambiar
            publicacion.FechaCierre = estado == EstadoPublicacion.Cerrada
                ? DateTime.UtcNow
                : null;

            await _context.SaveChangesAsync();

            return await ObtenerPublicacionAsync(id, refugioId);
        }


        ///<inheritdoc/>
        public async Task<List<PublicacionPublicaResponse>> ListarActivasAsync()
        {
            var publicaciones = await _context.Publicaciones
                .AsNoTracking()
                .Include(p => p.Mascota)!.ThenInclude(m => m!.Especie)
                .Include(p => p.Mascota)!.ThenInclude(m => m!.Refugio)
                .Where(p => p.Estado == EstadoPublicacion.Activa)
                .OrderByDescending(p => p.FechaPublicacion)
                .ToListAsync();

            return publicaciones.Select(PublicacionPublicaDTO).ToList();
        }

        ///<inheritdoc/>
        public async Task<PublicacionPublicaResponse?> ObtenerActivaAsync(Guid id)
        {
            var publicacion = await _context.Publicaciones
                .AsNoTracking()
                .Include(p => p.Mascota)!.ThenInclude(m => m!.Especie)
                .Include(p => p.Mascota)!.ThenInclude(m => m!.Refugio)
                .FirstOrDefaultAsync(p => p.Id == id && p.Estado == EstadoPublicacion.Activa);

            return publicacion is null ? null : PublicacionPublicaDTO(publicacion);
        }



        /// <summary>
        /// Pasa la entidad al DTO que ve el refugio
        /// </summary>
        private static PublicacionResponse PublicacionDTO(Publicacion p, int cantidadPostulaciones) => new()
        {
            Id = p.Id,
            MascotaId = p.MascotaId,
            MascotaNombre = p.Mascota?.Nombre ?? string.Empty,
            Titulo = p.Titulo,
            Descripcion = p.Descripcion,
            Ubicacion = p.Ubicacion,
            Tipo = p.Tipo,
            Estado = p.Estado,
            FotosUrlExtra = p.FotosUrlExtra,
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
            FotosUrlExtra = p.FotosUrlExtra,
            PlazoEstimado = p.PlazoEstimado,
            FechaPublicacion = p.FechaPublicacion,

            MascotaNombre = p.Mascota?.Nombre ?? string.Empty,
            EspecieNombre = p.Mascota?.Especie?.Nombre ?? string.Empty,
            Sexo = p.Mascota?.Sexo,
            Tamanio = p.Mascota?.Tamanio,
            EdadAproximadaMeses = p.Mascota?.FechaNacimientoAproximada.EdadEnMeses(),
            Vacunado = p.Mascota?.Vacunado ?? false,
            FotosUrl = p.Mascota?.FotosUrl,

            RefugioNombre = p.Mascota?.Refugio?.Nombre ?? string.Empty,
            RefugioLocalidad = p.Mascota?.Refugio?.Localidad
        };


    }
}
