using Microsoft.EntityFrameworkCore;
using TransitAR.Structures;
using TransitAR.Structures.Requests;

namespace TransitAR.Api.Services
{
    public class SeguimientoService : ISeguimientoService
    {
        private readonly TransitARContext _context;

        /// <summary>
        /// Inicializa el contexto
        /// </summary>
        /// <param name="context"></param>
        public SeguimientoService(TransitARContext context)
        {
            _context = context;
        }




        ///<inheritdoc/>
        public async Task<List<SeguimientoResponse>> ListarAsync(Guid usuarioId, Guid? refugioId, EstadoSeguimiento? estado, bool vencidos, Guid? mascotaId)
        {
            var consulta = ConsultaCompleta().AsNoTracking();

            //filtro para que solo lo vea el refugio y la persona
            if (refugioId != null)
                consulta = consulta.Where(s => s.Tenencia!.Mascota!.RefugioId == refugioId.Value);
            else
                consulta = consulta.Where(s => s.Tenencia!.Postulacion!.UsuarioId == usuarioId);

            //que tenga estado
            if (estado != null)
                consulta = consulta.Where(s => s.Estado == estado.Value);

            //vencido, es un pendiente que tiene fecha antes de hoy
            if (vencidos)
            {
                var hoy = DateTime.UtcNow.Date;
                consulta = consulta.Where(s => s.Estado == EstadoSeguimiento.Pendiente && s.FechaProgramada < hoy);
            }

            //filtro por mascota
            if (mascotaId != null)
                consulta = consulta.Where(s => s.Tenencia!.MascotaId == mascotaId.Value);

            var seguimientos = await consulta
                .OrderBy(s => s.FechaProgramada)
                .ToListAsync();

            return seguimientos.Select(SeguimientoDTO).ToList();
        }



        ///<inheritdoc/>
        public async Task<string?> CrearAsync(SeguimientoRequest request, Guid refugioId)
        {

            if (request?.TenenciaId == null || request.FechaProgramada == null)
                return "No existe la tenencia o la fecha del control.";

            if (request.FechaProgramada.Value.Date < DateTime.UtcNow.Date)
                return "La fecha del control tiene que ser de hoy para adelante.";

            var tenencia = await _context.Tenencias
                .Include(t => t.Mascota)
                .FirstOrDefaultAsync(t => t.Id == request.TenenciaId.Value);

            //reviso que sea una tenencia de este refugio o que este la mascota
            if (tenencia == null || tenencia.Mascota?.RefugioId != refugioId)
                return "No encontramos esa tenencia en tus mascotas.";

            //en caso de tenencia por transito, no se peude seguir observando
            if (tenencia.FechaFinReal != null)
                return "La tenencia ya termino: no se pueden agendar controles nuevos.";

            _context.Seguimientos.Add(new Seguimiento
            {
                Id = Guid.NewGuid(),
                TenenciaId = tenencia.Id,
                FechaProgramada = request.FechaProgramada.Value,
                Estado = EstadoSeguimiento.Pendiente
            });

            await _context.SaveChangesAsync();
            return null;
        }

        ///<inheritdoc/>
        public async Task<string?> CambiarEstadoAsync(Guid id, SeguimientoRequest request, Guid refugioId)
        {
            if (request?.Estado == null)
                return "No se encuentra el estado.";

            var nuevoEstado = request.Estado.Value;

            //Que exista el estado al que se quiera pasar
            if (nuevoEstado != EstadoSeguimiento.Realizado
                && nuevoEstado != EstadoSeguimiento.Cancelado
                && nuevoEstado != EstadoSeguimiento.Reprogramado)
                return "Un control solo puede pasar a Realizado, Cancelado o Reprogramado.";

            var seguimiento = await _context.Seguimientos
                .Include(s => s.Tenencia)!
                    .ThenInclude(t => t!.Mascota)
                .FirstOrDefaultAsync(s => s.Id == id);

            //Verifico que exista el control al que quiero cambiar el estado
            if (seguimiento?.Tenencia?.Mascota?.RefugioId != refugioId)
                return "No encontramos ese control en tus mascotas.";

            //Si termino queda guardado, solo lo puedo llegar a cambiar cuando este pendiente o vencido(que es un pendiente pero con fecha anterior a hoy)
            if (seguimiento.Estado != EstadoSeguimiento.Pendiente)
                return "Este control ya esta cerrado y no se puede modificar.";

            //Si es un transito, y termino no deberiamos poder seguir haciendo seguimientos
            if (seguimiento.Tenencia.FechaFinReal != null)
                return "La tenencia ya termino: su libreta es de solo lectura.";

            var ahora = DateTime.UtcNow;
            seguimiento.Estado = nuevoEstado;
            seguimiento.Observacion = request.Observacion?.Trim();

            if (nuevoEstado == EstadoSeguimiento.Realizado)
                seguimiento.FechaRealizada = ahora;

            if (nuevoEstado == EstadoSeguimiento.Reprogramado)
            {
                if (request.FechaProgramada == null || request.FechaProgramada.Value.Date < ahora.Date)
                    return "Para reprogramar hace falta una fecha nueva, de hoy para adelante";

                //el viejo queda registrado con la fecha original; el nuevo esta en la agenda
                _context.Seguimientos.Add(new Seguimiento
                {
                    Id = Guid.NewGuid(),
                    TenenciaId = seguimiento.TenenciaId,
                    FechaProgramada = request.FechaProgramada.Value,
                    Estado = EstadoSeguimiento.Pendiente
                });
            }

            //guardo el viejo y neuvo jutnos
            await _context.SaveChangesAsync();
            return null;
        }

        /// <summary>
        /// Consulta compelta con todos los datos que necesita el DTO: la mascota y su refugio, y la persona que tiene al animal
        /// </summary>
        private IQueryable<Seguimiento> ConsultaCompleta() =>
            _context.Seguimientos
                .Include(s => s.Tenencia)!
                    .ThenInclude(t => t!.Mascota)!
                        .ThenInclude(m => m!.Refugio)
                .Include(s => s.Tenencia)!
                    .ThenInclude(t => t!.Postulacion)!
                        .ThenInclude(p => p!.Usuario);

        /// <summary>
        /// Pasa la entidad al DTO. Los nombres salen de las navegaciones,no en el seguimiento
        /// </summary>
        private static SeguimientoResponse SeguimientoDTO(Seguimiento s) => new()
        {
            Id = s.Id,
            TenenciaId = s.TenenciaId,
            MascotaId = s.Tenencia?.MascotaId ?? Guid.Empty,
            PostulacionId = s.Tenencia?.PostulacionId ?? Guid.Empty,
            MascotaNombre = s.Tenencia?.Mascota?.Nombre ?? string.Empty,
            PersonaNombre = $"{s.Tenencia?.Postulacion?.Usuario?.Nombre} {s.Tenencia?.Postulacion?.Usuario?.Apellido}".Trim(),
            RefugioNombre = s.Tenencia?.Mascota?.Refugio?.Nombre ?? string.Empty,
            Modalidad = s.Tenencia?.Modalidad ?? default,
            FechaProgramada = s.FechaProgramada,
            FechaRealizada = s.FechaRealizada,
            Observacion = s.Observacion,
            Estado = s.Estado,
            Vencido = s.Estado == EstadoSeguimiento.Pendiente && s.FechaProgramada.Date < DateTime.UtcNow.Date
        };
    }
}
