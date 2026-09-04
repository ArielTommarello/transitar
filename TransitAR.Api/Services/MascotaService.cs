using Azure.Core;
using Microsoft.EntityFrameworkCore;
using TransitAR.Structures;

namespace TransitAR.Api.Services
{

    /// <summary>
    /// Implemebntacion de creacion y lectura de mascotas por refugios
    /// </summary>


    public class MascotaService : IMascotaService
    {

        private readonly TransitARContext _context;
        

        /// <summary>
        /// Inicializa el contexto
        /// </summary>
        /// <param name="context"></param>
        public MascotaService(TransitARContext context)
        {
            _context = context;
        }




        ///<inheritdoc/>
        public async Task<List<MascotaResponse>> ListarMascotasAsync(Guid refugioId)
        {
            var mascotas = await _context.Mascotas
                .Include(m => m.Especie)
                .Include(m => m.Condicion)
                .Where(m => m.RefugioId == refugioId)
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            return mascotas.Select(MascotaDTO).ToList();
        }

        ///<inheritdoc/>
        public async Task<MascotaResponse?> ObtenerMascotaAsync(Guid id, Guid refugioId)
        {
            var mascota = await _context.Mascotas
                .Include (m => m.Especie)
                .Include (m => m.Condicion)
                .FirstOrDefaultAsync(m => m.Id == id && m.RefugioId == refugioId);

            if (mascota == null) return null;
            return MascotaDTO(mascota);
        }




        ///<inheritdoc/>
        public async Task<MascotaResponse?> CrearMascotaAsync(MascotaRequest request, Guid refugioId)
        {
            if (request == null) return null;
            if (refugioId == Guid.Empty) return null;

            if (!await esValidoAsync(request))
                return null;

            var mascota = new Mascota
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre.Trim(),
                RefugioId = refugioId,
                EspecieId = request.EspecieId,
                CondicionId = request.CondicionId,
                Raza = request.Raza?.Trim(),
                Sexo = request.Sexo,
                FechaNacimientoAproximada = request.FechaNacimientoAproximada,
                Tamanio = request.Tamanio,
                Vacunado = request.Vacunado,
                Estado = EstadoMascota.EnRefugio,
                FotosUrl = request.FotosUrl,
                FechaAlta = DateTime.UtcNow

            };

            _context.Add(mascota);
            await _context.SaveChangesAsync();

            return await ObtenerMascotaAsync(mascota.Id, refugioId);

        }

        ///<inheritdoc/>
        public async Task<MascotaResponse?> ActualizarMascotaAsync(Guid id,MascotaRequest request, Guid refugioId)
        {
            if (request == null) return null;
            if (refugioId == Guid.Empty) return null;

            var mascota = await _context.Mascotas.FirstOrDefaultAsync(m => m.Id == id && m.RefugioId == refugioId);

            if(mascota == null) return null;    


            if (!await esValidoAsync(request))
                return null;

            mascota.Nombre = request.Nombre.Trim();
            mascota.EspecieId = request.EspecieId;
            mascota.CondicionId = request.CondicionId;
            mascota.Raza = request.Raza?.Trim();
            mascota.Sexo = request.Sexo;
            mascota.FechaNacimientoAproximada = request.FechaNacimientoAproximada;
            mascota.Tamanio = request.Tamanio;
            mascota.Vacunado = request.Vacunado;
            mascota.FotosUrl = request.FotosUrl;

            await _context.SaveChangesAsync();

            return await ObtenerMascotaAsync(mascota.Id, refugioId);

        }

        ///<inheritdoc/>
        public async Task<bool> EliminarMascotaAsync(Guid id,  Guid refugioId)
        {

            var mascota = await _context.Mascotas.FirstOrDefaultAsync(m => m.Id == id && m.RefugioId == refugioId);

            if (mascota == null) return false;

            _context.Mascotas.Remove(mascota);
            await _context.SaveChangesAsync();
            return true;

        }

        /// <summary>
        /// Verifico que la especie y condicion esten en nuestro catalogo o el que agrego el admin
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<bool> esValidoAsync(MascotaRequest request)
        {
            if(!await _context.Especies.AnyAsync(e => e.Id == request.EspecieId))
                return false;

            return await _context.Condiciones.AnyAsync(c => c.Id == request.CondicionId);
        }


        /// <summary>
        /// Obtengo el dto de la mascota para no tener toda la entidad, y poder calcular los valores, de esta manera menos cosas para cargar cuando se pidan
        /// </summary>
        /// <param name="mascota"></param>
        /// <returns></returns>
        private static MascotaResponse MascotaDTO(Mascota mascota) => new()
        {
            Id = mascota.Id,
            Nombre = mascota.Nombre,
            EspecieNombre = mascota.Especie?.Nombre ?? string.Empty,
            CondicionNombre = mascota.Condicion?.Nombre ?? string.Empty,
            Raza = mascota.Raza,
            Sexo = mascota.Sexo,
            FechaNacimientoAproximada = mascota.FechaNacimientoAproximada,
            EdadAproximadaMeses = CalcularEdadMeses(mascota.FechaNacimientoAproximada),
            Tamanio = mascota.Tamanio,
            Vacunado = mascota.Vacunado,
            Estado = mascota.Estado,
            FotosUrl = mascota.FotosUrl,
            FechaAlta = mascota.FechaAlta
        };

        /// <summary>
        /// Calcula la edad en meses a partir del dia de nacimiento, contra el dia actual . Error si no es static
        /// </summary>
        /// <param name="nacimiento"></param>
        /// <returns></returns>
        private static int? CalcularEdadMeses(DateTime? nacimiento)
        {
            if(nacimiento == null) return null;
            //dia actual
            var hoy = DateTime.UtcNow;

            //calculo los meses dependiendo el utc de nacimiento cargado contra el dia de hoy
            var meses = ((hoy.Year - nacimiento.Value.Year)*12) + hoy.Month - nacimiento.Value.Month;


            if(hoy.Day < nacimiento.Value.Day)
                meses--;

            return meses < 0 ? 0 : meses;

        }

    }
}
