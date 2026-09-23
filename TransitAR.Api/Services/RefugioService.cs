using TransitAR.Structures.Requests;
using TransitAR.Structures;
using Microsoft.EntityFrameworkCore;

namespace TransitAR.Api.Services
{
    /// <summary>
    /// Implementacion del perfil del refugio. Se identifica por el RefugioId que llega del token
    /// </summary>
    public class RefugioService : IRefugioService
    {

        private readonly TransitARContext _context;

        /// <summary>
        /// Inicializa el contexto
        /// </summary>
        /// <param name="context"></param>
        public RefugioService(TransitARContext context)
        {
            _context = context;
        }

        ///<inheritdoc/>
        public async Task<RefugioResponse?> ObtenerPerfilAsync(Guid refugioId)
        {
            var refugio = await _context.Refugios
                .AsNoTracking()
                .Include(r => r.Contactos)
                .FirstOrDefaultAsync(r => r.Id == refugioId);

            return refugio == null ? null : RefugioDTO(refugio);
        }

        ///<inheritdoc/>
        public async Task<RefugioResponse?> ActualizarPerfilAsync(RefugioRequest request, Guid refugioId)
        {
            if (request == null || refugioId == Guid.Empty)
                return null;

            var refugio = await _context.Refugios
                .Include(r => r.Contactos)
                .FirstOrDefaultAsync(r => r.Id == refugioId);

            if (refugio == null)
                return null;

            //Id, FechaAlta y Activo no se cambian
            refugio.Nombre = request.Nombre.Trim();
            refugio.Descripcion = request.Descripcion?.Trim();
            refugio.LogoUrl = request.LogoUrl?.Trim();
            refugio.Email = request.Email.Trim().ToLowerInvariant();
            refugio.Telefono = request.Telefono?.Trim();
            refugio.Direccion = request.Direccion?.Trim();
            refugio.Localidad = request.Localidad?.Trim();
            refugio.MensajeRechazoAutomatico = request.MensajeRechazoAutomatico?.Trim();

            SincronizarContactos(refugio, request);

            await _context.SaveChangesAsync();

            return await ObtenerPerfilAsync(refugioId);
        }

        /// <summary>
        /// Solo se tocan los contactos que se borraron o los que se agregan. (Como perfil Postulante)
        /// </summary>
        private void SincronizarContactos(Refugio refugio, RefugioRequest request)
        {

            var enviados = request.Contactos ?? new List<ContactoRequest>();

            foreach (var existente in refugio.Contactos.ToList())
            {
                //var enviado = request.Contactos.FirstOrDefault(c => c.Tipo == existente.Tipo);
                var enviado = enviados.FirstOrDefault(c => c.Tipo == existente.Tipo);

                if (enviado == null)
                    _context.ContactosRefugio.Remove(existente);
                else
                    existente.Url = enviado.Url.Trim();
            }

            foreach (var enviado in request.Contactos)
            {
                if (!refugio.Contactos.Any(c => c.Tipo == enviado.Tipo))
                {
                    _context.ContactosRefugio.Add(new ContactoRefugio
                    {
                        Id = Guid.NewGuid(),
                        RefugioId = refugio.Id,
                        Tipo = enviado.Tipo,
                        Url = enviado.Url.Trim()
                    });
                }
            }
        }

        /// <summary>
        /// Pasa la entidad al DTO de salida
        /// </summary>
        private static RefugioResponse RefugioDTO(Refugio r) => new()
        {
            Id = r.Id,
            Nombre = r.Nombre,
            Descripcion = r.Descripcion,
            LogoUrl = r.LogoUrl,
            Email = r.Email,
            Telefono = r.Telefono,
            Direccion = r.Direccion,
            Localidad = r.Localidad,
            MensajeRechazoAutomatico = r.MensajeRechazoAutomatico,
            FechaAlta = r.FechaAlta,
            Contactos = r.Contactos
                .Select(c => new ContactoResponse { Tipo = c.Tipo, Url = c.Url })
                .ToList()
        };

    }
}
