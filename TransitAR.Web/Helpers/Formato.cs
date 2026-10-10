using System.Text.Json;
using TransitAR.Structures;

namespace TransitAR.Web.Helpers
{
    public class Formato
    {

        /// <summary>
        /// formato para el tipo de publicacion
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public static string Tipo(TipoPublicacion tipo) => tipo switch
        {
            TipoPublicacion.Adopcion => "Adopción",
            TipoPublicacion.Transito => "Tránsito",
            _ => string.Empty
        };

        /// <summary>
        /// formato para el tamaño del animal
        /// </summary>
        /// <param name="tamanio"></param>
        /// <returns></returns>
        public static string Tamanio(Tamanio? tamanio) => tamanio switch
        {
            Structures.Tamanio.Chico => "Chico",
            Structures.Tamanio.Mediano => "Mediano",
            Structures.Tamanio.Grande => "Grande",
            _ => "Tamaño sin datos"
        };

        /// <summary>
        /// formato para el sexo del animal
        /// </summary>
        /// <param name="sexo"></param>
        /// <returns></returns>
        public static string Sexo(Sexo? sexo) => sexo switch
        {
            Structures.Sexo.Macho => "Macho",
            Structures.Sexo.Hembra => "Hembra",
            _ => "Sexo sin datos"
        };

        /// <summary>
        /// edad , meses menos de un año y sino años
        /// </summary>
        public static string Edad(int? meses)
        {
            if (meses == null)
                return "Edad desconocida";

            if (meses < 12)
                return meses == 1 ? "1 mes" : $"{meses} meses";

            var anios = meses.Value / 12;
            return anios == 1 ? "1 año" : $"{anios} años";
        }

        /// <summary>
        /// Primera foto de la lista para el animal, sino null si no hay
        /// </summary>
        public static string? PrimeraFoto(string? fotosJson)
        {
            if (string.IsNullOrWhiteSpace(fotosJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<List<string>>(fotosJson)?.FirstOrDefault();
            }
            catch (JsonException)
            {
                return null;
            }
        }


    }
}
