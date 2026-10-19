using Azure.Core;
using Microsoft.EntityFrameworkCore;
using TransitAR.Api.Extensions;
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
                .Include(m => m.Fotos)
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
                .Include(m => m.Fotos)
                .FirstOrDefaultAsync(m => m.Id == id && m.RefugioId == refugioId);

            if (mascota == null) return null;
            return MascotaDTO(mascota);
        }


        ///<inheritdoc/>
        public async Task<MascotaResponse?> ObtenerMascotaDeTenenciaAsync(Guid tenenciaId, Guid usuarioId)
        {
            //la tenencia tiene que ser de esta persona (por su postulacion)
            var mascotaId = await _context.Tenencias
                .Where(t => t.Id == tenenciaId && t.Postulacion!.UsuarioId == usuarioId)
                .Select(t => (Guid?)t.MascotaId)
                .FirstOrDefaultAsync();

            if (mascotaId == null)
                return null;

            var mascota = await _context.Mascotas
                .AsNoTracking()
                .Include(m => m.Especie)
                .Include(m => m.Condicion)
                .Include(m => m.Fotos)
                .FirstOrDefaultAsync(m => m.Id == mascotaId);

            return mascota == null ? null : MascotaDTO(mascota);
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


        //FOTOS ADJUNTAS MASCOTA

        ///<inheritdoc/>
        public async Task<string?> AgregarFotosAsync(Guid mascotaId, Guid refugioId, List<ArchivoAdjunto> fotos, int maximo)
        {
            if (!await _context.Mascotas.AnyAsync(m => m.Id == mascotaId && m.RefugioId == refugioId))
                return "No encontramos esa mascota en tu refugio.";

            //cantidad de fotos (count)
            var cantidad = await _context.FotosMascota.CountAsync(f => f.MascotaId == mascotaId);

            //error cantidad fotos
            if (cantidad + fotos.Count > maximo)
                return $"Una mascota puede tener hasta {maximo} fotos. Ya tiene {cantidad}.";

            //cola , se agregan las neuvas al final
            for (var i = 0; i < fotos.Count; i++)
            {
                _context.FotosMascota.Add(new FotoMascota
                {
                    Id = Guid.NewGuid(),
                    MascotaId = mascotaId,
                    Orden = cantidad + i,
                    ContentType = fotos[i].ContentType,
                    Contenido = fotos[i].Contenido
                });
            }

            await _context.SaveChangesAsync();
            return null;
        }

        ///<inheritdoc/>
        public async Task<bool> BorrarFotoAsync(Guid mascotaId, Guid fotoId, Guid refugioId)
        {
            var fotos = await FotosDelRefugioAsync(mascotaId, refugioId);

            var foto = fotos.FirstOrDefault(f => f.Id == fotoId);
            if (foto == null)
                return false;

            _context.FotosMascota.Remove(foto);
            fotos.Remove(foto);

            //funcion auxiliar apra acomodar las que quedaron
            Reordenar(fotos);

            await _context.SaveChangesAsync();
            return true;
        }

        ///<inheritdoc/>
        public async Task<bool> HacerPrincipalAsync(Guid mascotaId, Guid fotoId, Guid refugioId)
        {
            var fotos = await FotosDelRefugioAsync(mascotaId, refugioId);

            var foto = fotos.FirstOrDefault(f => f.Id == fotoId);
            if (foto == null)
                return false;

            //paso a primera la que se eligio, y modifico los numeros del orden de las otras
            fotos.Remove(foto);
            fotos.Insert(0, foto);
            Reordenar(fotos);

            await _context.SaveChangesAsync();
            return true;
        }

        ///<inheritdoc/>
        public async Task<ArchivoAdjunto?> ObtenerFotoAsync(Guid fotoId)
        {
            return await _context.FotosMascota
                .AsNoTracking()
                .Where(f => f.Id == fotoId)
                .Select(f => new ArchivoAdjunto
                {
                    NombreArchivo = "foto",
                    ContentType = f.ContentType,
                    Contenido = f.Contenido
                })
                .FirstOrDefaultAsync();
        }


        /// <summary>
        /// Fotos de una mascota del refugio, en orden
        /// </summary>
        private async Task<List<FotoMascota>> FotosDelRefugioAsync(Guid mascotaId, Guid refugioId) =>
            await _context.FotosMascota
                .Where(f => f.MascotaId == mascotaId && f.Mascota!.RefugioId == refugioId)
                .OrderBy(f => f.Orden)
                .ToListAsync();

        /// <summary>
        /// Reordena el orden de las fotos 
        /// </summary>
        private static void Reordenar(List<FotoMascota> fotos)
        {
            for (var i = 0; i < fotos.Count; i++)
                fotos[i].Orden = i;
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
            EdadAproximadaMeses = mascota.FechaNacimientoAproximada.EdadEnMeses(),
            Tamanio = mascota.Tamanio,
            Vacunado = mascota.Vacunado,
            FotosIds = IdsDeFotos(mascota),
            Estado = mascota.Estado,            
            FechaAlta = mascota.FechaAlta
        };


        /// <summary>
        /// defino el orden de las fotos de la masctoa , lo uso en varios serivcios
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        internal static List<Guid> IdsDeFotos(Mascota? m) =>
            m?.Fotos.OrderBy(f => f.Orden).Select(f => f.Id).ToList() ?? new List<Guid>();
    }
}
