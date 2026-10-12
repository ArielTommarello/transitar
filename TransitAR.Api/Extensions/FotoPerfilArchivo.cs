using TransitAR.Structures;

namespace TransitAR.Api.Extensions
{
    /// <summary>
    /// Validacion y lectura de las fotos de perfil (usuario y refugio)
    /// </summary>
    public static class FotoPerfilArchivo
    {
        private static readonly string[] TiposPermitidos = { "image/jpeg", "image/png", "image/webp" };

        /// <summary>
        /// Valido la lectura de la foto de perfil (usuario y refugio) 
        /// </summary>
        /// <param name="foto"></param>
        /// <param name="maxMB"></param>
        /// <returns></returns>
        public static string? Validar(IFormFile foto, int maxMB)
        {
            if (foto.Length == 0)
                return "La foto esta vacia.";

            if (foto.Length > (long)maxMB * 1024 * 1024)
                return $"La foto supera los {maxMB} MB permitidos.";

            if (!TiposPermitidos.Contains(foto.ContentType.Trim().ToLowerInvariant()))
                return "La foto tiene que ser jpg, png o webp.";

            return null;
        }

        /// <summary>
        /// Copio lo de la fotos del chat, trasnformacion de IFORMFILE a bytes
        /// </summary>
        /// <param name="foto"></param>
        /// <returns></returns>
        public static async Task<ArchivoAdjunto> LeerAsync(IFormFile foto)
        {
            using var memoria = new MemoryStream();
            await foto.CopyToAsync(memoria);

            return new ArchivoAdjunto
            {
                NombreArchivo = foto.FileName,
                ContentType = foto.ContentType.Trim().ToLowerInvariant(),
                Contenido = memoria.ToArray()
            };
        }
    }
}
