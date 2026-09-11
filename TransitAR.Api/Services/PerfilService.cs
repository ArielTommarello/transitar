using Microsoft.EntityFrameworkCore;
using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    /// <summary>
    /// Implementacion del perfil de requisitos del postulante. El perfil es uno solo por usuario y se identifica por el UsuarioId que llega del token
    /// </summary>
    public class PerfilService : IPerfilService
    {

        private readonly TransitARContext _context;

        /// <summary>
        /// Inicializa el contexto
        /// </summary>
        /// <param name="context"></param>
        public PerfilService(TransitARContext context)
        {
            _context = context;
        }

        ///<inheritdoc/>
        public async Task<PerfilResponse?> ObtenerPerfilAsync(Guid usuarioId)
        {
            var perfil = await _context.PerfilPostulantes
                .AsNoTracking()
                .Include(p => p.Contactos)
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);

            return perfil is null ? null : PerfilDTO(perfil);
        }

        ///<inheritdoc/>
        public async Task<PerfilResponse?> CrearPerfilAsync(PerfilRequest request, Guid usuarioId)
        {
            if (request == null || usuarioId == Guid.Empty)
                return null;

            //un usuario tiene un solo perfil
            if (await _context.PerfilPostulantes.AnyAsync(p => p.UsuarioId == usuarioId))
                return null;

            var perfil = new PerfilPostulante
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Seleccion = request.Seleccion,
                FotoUrl = request.FotoUrl?.Trim(),
                TipoVivienda = request.TipoVivienda.Trim(),
                TienePatio = request.TienePatio,
                PatioCerrado = request.PatioCerrado,
                TieneOtrasMascotas = request.TieneOtrasMascotas,
                DetalleOtrasMascotas = request.DetalleOtrasMascotas?.Trim(),
                HorasSoloPorDia = request.HorasSoloPorDia,
                CercaniaVeterinaria = request.CercaniaVeterinaria?.Trim(),
                ExperienciaPrevia = request.ExperienciaPrevia?.Trim(),
                MotivoPostulacion = request.MotivoPostulacion.Trim(),
                FechaCompletado = DateTime.UtcNow
            };

            foreach (var contacto in request.Contactos)
            {
                perfil.Contactos.Add(new ContactoPostulante
                {
                    Id = Guid.NewGuid(),
                    PerfilPostulanteId = perfil.Id,
                    Tipo = contacto.Tipo,
                    Url = contacto.Url.Trim()
                });
            }

            _context.PerfilPostulantes.Add(perfil);
            await _context.SaveChangesAsync();

            return await ObtenerPerfilAsync(usuarioId);
        }

        ///<inheritdoc/>
        public async Task<PerfilResponse?> ActualizarPerfilAsync(PerfilRequest request, Guid usuarioId)
        {
            if (request == null || usuarioId == Guid.Empty)
                return null;

            var perfil = await _context.PerfilPostulantes
                .Include(p => p.Contactos)
                .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);

            if (perfil is null)
                return null;

            //UsuarioId y FechaCompletado no se cambian
            perfil.Seleccion = request.Seleccion;
            perfil.FotoUrl = request.FotoUrl?.Trim();
            perfil.TipoVivienda = request.TipoVivienda.Trim();
            perfil.TienePatio = request.TienePatio;
            perfil.PatioCerrado = request.PatioCerrado;
            perfil.TieneOtrasMascotas = request.TieneOtrasMascotas;
            perfil.DetalleOtrasMascotas = request.DetalleOtrasMascotas?.Trim();
            perfil.HorasSoloPorDia = request.HorasSoloPorDia;
            perfil.CercaniaVeterinaria = request.CercaniaVeterinaria?.Trim();
            perfil.ExperienciaPrevia = request.ExperienciaPrevia?.Trim();
            perfil.MotivoPostulacion = request.MotivoPostulacion.Trim();

            SincronizarContactos(perfil, request);

            await _context.SaveChangesAsync();

            return await ObtenerPerfilAsync(usuarioId);
        }

        /// <summary>
        /// Solo se tocan los contactos qe se borraron o si se agrega nuevos
        /// </summary>
        private void SincronizarContactos(PerfilPostulante perfil, PerfilRequest request)
        {
            foreach (var existente in perfil.Contactos.ToList())
            {
                var enviado = request.Contactos.FirstOrDefault(c => c.Tipo == existente.Tipo);

                if (enviado is null)
                    _context.ContactoPostulantes.Remove(existente);
                else
                    existente.Url = enviado.Url.Trim();
            }

            foreach (var enviado in request.Contactos)
            {
                if (!perfil.Contactos.Any(c => c.Tipo == enviado.Tipo))
                {
                    _context.ContactoPostulantes.Add(new ContactoPostulante
                    {
                        Id = Guid.NewGuid(),
                        PerfilPostulanteId = perfil.Id,
                        Tipo = enviado.Tipo,
                        Url = enviado.Url.Trim()
                    });
                }
            }
        }

        /// <summary>
        /// Obtengo el dto del perfil para no tener toda la entidad, y poder calcular los valores, de esta manera menos cosas para cargar cuando se pidan
        /// </summary>
        private static PerfilResponse PerfilDTO(PerfilPostulante p) => new()
        {
            Id = p.Id,
            Seleccion = p.Seleccion,
            FotoUrl = p.FotoUrl,
            //podria dar null, entonces lo exijo en la api y no tengo que cambiar todo el DTO , en caso de que no quieran tener esto obligatorio los del refugio
            TipoVivienda = p.TipoVivienda ?? string.Empty,
            TienePatio = p.TienePatio,
            PatioCerrado = p.PatioCerrado,
            TieneOtrasMascotas = p.TieneOtrasMascotas,
            DetalleOtrasMascotas = p.DetalleOtrasMascotas,
            HorasSoloPorDia = p.HorasSoloPorDia,
            CercaniaVeterinaria = p.CercaniaVeterinaria,
            ExperienciaPrevia = p.ExperienciaPrevia,
            //podria dar null, entonces lo exijo en la api y no tengo que cambiar todo el DTO , en caso de que no quieran tener esto obligatorio los del refugio
            MotivoPostulacion = p.MotivoPostulacion ?? string.Empty,
            FechaCompletado = p.FechaCompletado,
            Contactos = p.Contactos
                .Select(c => new ContactoResponse { Tipo = c.Tipo, Url = c.Url })
                .ToList()
        };



    }
}
