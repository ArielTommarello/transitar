using TransitAR.Structures.Requests;
using TransitAR.Structures;
using Microsoft.EntityFrameworkCore;
using System;


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

            foreach (var enviado in enviados)
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


        ///<inheritdoc/>
        public async Task<bool> CambiarFotoAsync(Guid refugioId, ArchivoAdjunto foto)
        {
            var refugio = await _context.Refugios.FirstOrDefaultAsync(r => r.Id == refugioId);
            if (refugio == null)
                return false;

            //remplazo la anterior si ya hay una
            refugio.FotoPerfil = foto.Contenido;
            refugio.FotoContentType = foto.ContentType;

            await _context.SaveChangesAsync();
            return true;
        }

        ///<inheritdoc/>
        public async Task<ArchivoAdjunto?> ObtenerFotoAsync(Guid refugioId)
        {
            //EFCORE para traer osol colimnas de fotos y no todo
            return await _context.Refugios
                .AsNoTracking()
                .Where(r => r.Id == refugioId && r.FotoPerfil != null)
                .Select(r => new ArchivoAdjunto
                {
                    NombreArchivo = "logo",
                    ContentType = r.FotoContentType ?? "image/jpeg",
                    Contenido = r.FotoPerfil!
                })
                .FirstOrDefaultAsync();
        }


        #region Equipo

        ///<inheritdoc/>
        public async Task<List<MiembroEquipoResponse>> ListarEquipoAsync(Guid refugioId)
        {
            var equipo = await _context.Usuarios
               .AsNoTracking()
               .Where(u => u.RefugioId == refugioId)
               .OrderBy(u => u.RolRefugio).ThenBy(u => u.Nombre)
               .ToListAsync();

            return equipo.Select(MiembroDTO).ToList();
        }


        ///<inheritdoc/>
        public async Task<ColaboradorResult> AgregarColaboradorAsync(Guid fundadorId, Guid refugioId, ColaboradorRequest request)
        {
            if(request == null)
                return new ColaboradorResult { Error = "Datos inválidos." };

            if(!await EsFundadorAsync(fundadorId, refugioId))
                return new ColaboradorResult { Error = "Solo el fundador puede administrar el equipo." };

            var email = request.Email.Trim().ToLowerInvariant();

            
            if(await _context.Usuarios.AnyAsync(u => u.Email == email))
                return new ColaboradorResult { Error = "Ya existe una cuenta registrada con este mail." };


            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                Telefono = request.Telefono?.Trim(),
                Rol = RolUsuario.Refugio,
                RolRefugio = RolRefugio.Colaborador,
                RefugioId = refugioId,
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                //arreglo de error doble click o email registrado al mismo teimpo 
                if (await _context.Usuarios.AnyAsync(u => u.Email == email))
                    return new ColaboradorResult { Error = "Ya existe una cuenta registrada con este mail." };

                throw;
            }

            return new ColaboradorResult { Colaborador = MiembroDTO(usuario) };

        }


        ///<inheritdoc/>
        public async Task<string?> CambiarEstadoColaboradorAsync(Guid fundadorId, Guid refugioId, Guid colaboradorId, bool activo)
        {
            if (!await EsFundadorAsync(fundadorId, refugioId))
                return "Solo el fundador puede administrar el equipo.";

            var colaborador = await ColaboradorDelRefugioAsync(colaboradorId, refugioId);

            if (colaborador == null)
                return  "No encontramos ese colaborador en tu refugio.";

            colaborador.Activo = activo;

            await _context.SaveChangesAsync();
            return null;

        }


        ///<inheritdoc/>
        public async Task<string?> BlanquearPasswordColaboradorAsync(Guid fundadorId, Guid refugioId, Guid colaboradorId, string passwordNueva)
        {
            if (!await EsFundadorAsync(fundadorId, refugioId))
                return "Solo el fundador puede administrar el equipo.";

            var colaborador = await ColaboradorDelRefugioAsync(colaboradorId, refugioId);

            if (colaborador == null)
                return "No encontramos ese colaborador en tu refugio.";

            colaborador.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordNueva);

            await _context.SaveChangesAsync();
            return null;
        }


        /// <summary>
        /// Si el usuario es fundador activo del refugio. Rol refugio no esta en el tokem, lo busco en la base
        /// </summary>
        /// <param name="usuarioId"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        private async Task<bool> EsFundadorAsync(Guid usuarioId, Guid refugioId) =>
            await _context.Usuarios.AnyAsync(u => u.Id == usuarioId
                && u.RefugioId == refugioId
                && u.RolRefugio == RolRefugio.Fundador
                && u.Activo);


        /// <summary>
        /// Revisa si es colaborador del refugio
        /// </summary>
        /// <param name="colaboradorId"></param>
        /// <param name="refugioId"></param>
        /// <returns></returns>
        private async Task<Usuario?> ColaboradorDelRefugioAsync(Guid colaboradorId, Guid refugioId) =>
         await _context.Usuarios.FirstOrDefaultAsync(c => c.Id == colaboradorId
             && c.RefugioId == refugioId
             && c.RolRefugio == RolRefugio.Colaborador);

        #endregion


        /// <summary>
        /// Pasa la entidad al DTO de salida
        /// </summary>
        private static RefugioResponse RefugioDTO(Refugio r) => new()
        {
            Id = r.Id,
            Nombre = r.Nombre,
            Descripcion = r.Descripcion,
            FotoPerfil = r.FotoPerfil != null,
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

        /// <summary>
        /// Pasa el usuario al DTO del equipo. No pone PasswordHash
        /// </summary>
        private static MiembroEquipoResponse MiembroDTO(Usuario u) => new()
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Email = u.Email,
            Telefono = u.Telefono,
            RolRefugio = u.RolRefugio,
            Activo = u.Activo,
            FechaAlta = u.FechaAlta
        };



    }
}
