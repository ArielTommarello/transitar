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
        /// Todas las fotos de una o mas las de la mascota que sumo la publicacion.Puede estar vacia
        /// </summary>
        public static List<string> Fotos(params string?[] listasJson)
        {
            var fotos = new List<string>();

            foreach (var json in listasJson)
            {
                if (string.IsNullOrWhiteSpace(json))
                    continue;

                try
                {
                    fotos.AddRange(JsonSerializer.Deserialize<List<string>>(json) ?? new());
                }
                catch (JsonException)
                {
                   //si guardo mal una lista no romep las demas
                }
            }

            return fotos;
        }


        /// <summary>
        /// Primera foto de la lista para el animal, sino null si no hay
        /// </summary>
        public static string? PrimeraFoto(string? fotosJson) => 
            Fotos(fotosJson).FirstOrDefault();


        //Arreglo de formato de hora UTC

        /// <summary>
        /// Zona horaria de Argentina (-3)
        /// </summary>
        private static readonly TimeZoneInfo ZonaArgentina =
            TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");

        /// <summary>
        /// Un momento guardado en UTC, en hora argentina con fecha y hora. Para mensajes, lecturas, publicaciones
        /// </summary>
        public static string Instante(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(utc, ZonaArgentina).ToString("dd/MM/yyyy HH:mm");

        /// <summary>
        /// Solo la fecha en UTC, en hora argentina
        /// </summary>
        public static string FechaDe(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(utc, ZonaArgentina).ToString("dd/MM/yyyy");

        /// <summary>
        /// Un dia del calendario que eligio una persona (un control, una devolucion pactada) Se muestra como viene, pero con formato de vista argentina
        /// </summary>
        public static string Dia(DateTime dia) => dia.ToString("dd/MM/yyyy");



        //FORMATO PARA POSTULACIONES
        /// <summary>
        /// Como ve el postulante el estado de la postulacion
        /// </summary>
        /// <param name="estado"></param>
        /// <returns></returns>
        public static string Estado(EstadoPostulacion estado) => estado switch
        {
            EstadoPostulacion.Pendiente => "En evaluación",
            EstadoPostulacion.EnEspera => "En espera",
            EstadoPostulacion.Aceptada => "¡Te eligieron!",
            EstadoPostulacion.Rechazada => "No fuiste seleccionado",
            EstadoPostulacion.Retirada => "Retirada",
            _ => string.Empty
        };

        /// <summary>
        /// Regla para color de bootstrap para los estado de postulacion (sigo la regla en todo el programa)
        /// </summary>
        /// <param name="estado"></param>
        /// <returns></returns>
        public static string ColorEstado(EstadoPostulacion estado) => estado switch
        {
            EstadoPostulacion.Pendiente => "bg-warning text-dark",
            EstadoPostulacion.EnEspera => "bg-info text-dark",
            EstadoPostulacion.Aceptada => "bg-success",
            EstadoPostulacion.Rechazada => "bg-danger",
            _ => "bg-secondary"
        };

        /// <summary>
        /// Aclaraciones para el postulante en caso de respuesta en Estado (Rechazada uso de la api) 
        /// </summary>
        /// <param name="estado"></param>
        /// <returns></returns>
        public static string? AclaracionEstado(EstadoPostulacion estado) => estado switch
        {
            EstadoPostulacion.Pendiente => "El refugio todavía no la revisó.",
            EstadoPostulacion.EnEspera => "El refugio está avanzando con otra persona. Si no se concreta, puede volver a vos.",
            EstadoPostulacion.Aceptada => "Coordiná la entrega con el refugio.",
            _ => null
        };
    }
}
