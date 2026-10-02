using Microsoft.EntityFrameworkCore;
using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    public class AdminService : IAdminService
    {
        private readonly TransitARContext _context;

        /// <summary>
        /// Inicializa el contexto
        /// </summary>
        /// <param name="context"></param>
        public AdminService(TransitARContext context)
        {
            _context = context;
        }

        #region Usuario

        ///<inheritdoc/>
        public async Task<List<UsuarioAdminResponse>> ListarUsuariosAsync(RolUsuario? rol, bool? activo)
        {
            var consulta = _context.Usuarios.AsNoTracking();

            if (rol != null)
                consulta = consulta.Where(u => u.Rol == rol.Value);

            if (activo != null)
                consulta = consulta.Where(u => u.Activo == activo.Value);

            var usuarios = await consulta
                .OrderBy(u => u.Apellido)
                .ThenBy(u => u.Nombre)
                .ToListAsync();

            return usuarios.Select(UsuarioDTO).ToList();
        }


        ///<inheritdoc/>
        public async Task<string?> CambiarEstadoUsuarioAsync(Guid id, bool activo)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return "No encontramos ese usuario.";

            //No se puede bloquear el estado de un admnistrador
            if (usuario.Rol == RolUsuario.Admin)
                return "No se puede cambiar el estado de un administrador.";

            usuario.Activo = activo;
            await _context.SaveChangesAsync();

            return null;
        }
        #endregion

        #region Refugio
        ///<inheritdoc/>
        public async Task<List<RefugioAdminResponse>> ListarRefugiosAsync(bool? activo)
        {
            var consulta = _context.Refugios.AsNoTracking();

            if (activo != null)
                consulta = consulta.Where(r => r.Activo == activo.Value);

            var refugios = await consulta
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            return refugios.Select(RefugioDTO).ToList();
        }

        ///<inheritdoc/>
        public async Task<string?> CambiarEstadoRefugioAsync(Guid id, bool activo)
        {
            var refugio = await _context.Refugios.FirstOrDefaultAsync(r => r.Id == id);

            if (refugio == null)
                return "No encontramos ese refugio.";

            refugio.Activo = activo;
            await _context.SaveChangesAsync();

            return null;
        }
        #endregion

        /// <summary>
        /// Pasa el usuario al DTO que ve el admin
        /// </summary>
        private static UsuarioAdminResponse UsuarioDTO(Usuario u) => new()
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Email = u.Email,
            Rol = u.Rol,
            RefugioId = u.RefugioId,
            Activo = u.Activo,
            FechaAlta = u.FechaAlta
        };

        /// <summary>
        /// Pasa el refugio al DTO que ve el admin
        /// </summary>
        private static RefugioAdminResponse RefugioDTO(Refugio r) => new()
        {
            Id = r.Id,
            Nombre = r.Nombre,
            Email = r.Email,
            Localidad = r.Localidad,
            Activo = r.Activo,
            FechaAlta = r.FechaAlta
        };
    }
}
