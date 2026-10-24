using System.Text.Json;
using TransitAR.Structures;
using TransitAR.Structures.Requests;

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
        public static string Edad(int? meses, int? dias)
        {
            if (meses == null)
                return "Edad desconocida";

            if (dias != null && dias < LimiteDiasCachorro)
                return Dias(dias.Value);

            //En caso de usar fecha de nacimiento aproximada , pasados los 60 dias s emuestra como meses.
            var mesesMostrar = dias != null ? Math.Max(meses.Value, 2) : meses.Value;

            if (mesesMostrar < 12)
                return mesesMostrar == 1 ? "1 mes" : $"{mesesMostrar} meses";


            var anios = mesesMostrar / 12;
            return anios == 1 ? "1 año" : $"{anios} años";
        }


        /// <summary>
        /// Hasta cuantos dias la edad se muestra en dias
        /// </summary>
        private const int LimiteDiasCachorro = 60;

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


        /// <summary>
        /// Etiqueta de la card de una tenencia como la ve el postulante
        /// </summary>
        public static string EtiquetaTenencia(HistorialTenenciaResponse t)
        {
            if (t.FechaFinReal != null)
                return t.Modalidad == TipoPublicacion.Adopcion ? "Adopción devuelta" : "Devuelto";

            if (t.Modalidad == TipoPublicacion.Adopcion)
                return "Adoptado";

            return t.VencidaEnCurso ? "Devolución pendiente" : "En tránsito";
        }

        /// <summary>
        /// Diferenciacion de colores de la etiqueta
        /// </summary>
        public static string ColorTenencia(HistorialTenenciaResponse t)
        {
            if (t.FechaFinReal != null)
                return t.Modalidad == TipoPublicacion.Adopcion ? "bg-danger" : "bg-secondary";

            if (t.Modalidad == TipoPublicacion.Adopcion)
                return "bg-success";

            return t.VencidaEnCurso ? "bg-warning text-dark" : "bg-info text-dark";
        }

        /// <summary>
        /// Texto que explica en que quedo la tenencia
        /// </summary>
        public static string TextoTenencia(HistorialTenenciaResponse t)
        {
            //Si volvio al refugio
            if (t.FechaFinReal != null)
                return t.Modalidad == TipoPublicacion.Adopcion
                    ? $"La adopción terminó: volvió al refugio el {FechaDe(t.FechaFinReal.Value)}, después de {Dias(t.DuracionEnDias)}."
                    : $"Estuvo con vos {Dias(t.DuracionEnDias)}, hasta el {FechaDe(t.FechaFinReal.Value)}.";

            //adopcion en curso, directa o convertida desde un transito
            if (t.Modalidad == TipoPublicacion.Adopcion)
                return t.FechaConversion != null
                    ? $"Empezó como tránsito y lo adoptaste el {FechaDe(t.FechaConversion.Value)}."
                    : $"Es parte de tu familia desde el {FechaDe(t.FechaInicio)}.";

            //transito en curso
            if (t.VencidaEnCurso)
                return "La fecha pactada ya pasó. Coordiná la devolución con el refugio.";

            //la fecha pactada es un dia del calendario hacia adelante
            return t.FechaFinEstimada != null
                ? $"Hasta el {Dia(t.FechaFinEstimada.Value)}."
                : "Sin fecha de devolución pactada.";
        }

        /// <summary>
        /// Veredicto final  que indico el refugio al cerrar (null si la tenencia sigue en curso)
        /// </summary>
        public static string? VeredictoTenencia(HistorialTenenciaResponse t) => t.FinalizoBien switch
        {
            true => "El refugio indicó que terminó bien.",
            false => "El refugio indicó que hubo inconvenientes.",
            null => null
        };

        /// <summary>
        /// Cantidad de dias conversion para la teneinca
        /// </summary>
        public static string Dias(int dias) => dias == 1 ? "1 día" : $"{dias} días";


        //TARJETA TENENCIAS

        /// <summary>
        /// Estado del control de un desguimeinto. Pendiente con fecha pasaada es vencido
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string EstadoControl(SeguimientoResponse s) => s.Estado switch
        {
            EstadoSeguimiento.Pendiente => s.Vencido ? "Vencido" : "Programado",
            EstadoSeguimiento.Realizado => "Realizado",
            EstadoSeguimiento.Cancelado => "Cancelado",
            EstadoSeguimiento.Reprogramado => "Reprogramado",
            _ => string.Empty
        };

        /// <summary>
        /// Colores para el estado
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ColorControl(SeguimientoResponse s) => s.Estado switch
        {
            EstadoSeguimiento.Pendiente => s.Vencido ? "bg-warning text-dark" : "bg-info text-dark",
            EstadoSeguimiento.Realizado => "bg-success",
            _ => "bg-secondary"
        };


        //MASCOTAS DEL REFUGIO

        /// <summary>
        /// Leyenda del estado de una mascota
        /// </summary>
        /// <param name="estado"></param>
        /// <returns></returns>
        public static string EstadoMascota(EstadoMascota estado) => estado switch
        {
            //uso structures.EstadoMascota, porque el metodo es igual al enum, y por alguna razon tomaba el metodo y no el enum
            Structures.EstadoMascota.EnRefugio => "En refugio",
            Structures.EstadoMascota.EnTransito => "En tránsito",
            Structures.EstadoMascota.Adoptada => "Adoptada",
            _ => string.Empty
        };


        /// <summary>
        /// Colores para los estado de una mascota
        /// </summary>
        /// <param name="estado"></param>
        /// <returns></returns>
        public static string ColorEstadoMascota(EstadoMascota estado) => estado switch
        {
            //uso structures.EstadoMascota, porque el metodo es igual al enum, y por alguna razon tomaba el metodo y no el enum
            Structures.EstadoMascota.EnRefugio => "bg-primary",
            Structures.EstadoMascota.EnTransito => "bg-info text-dark",
            Structures.EstadoMascota.Adoptada => "bg-success",
            _ => "bg-secondary"
        };

        /// <summary>
        /// Fecha de nacimiento aprox a partid de la edad que puso el refugio. Null si no cargaron nunga de las cosas y la dejo desconocida
        /// </summary>
        /// <param name="anios"></param>
        /// <param name="meses"></param>
        /// <returns></returns>
        public static DateTime? FechaDesdeEdad(int? anios, int? meses, int? dias)
        {
            if (anios == null && meses == null && dias == null)
                return null;

            //es un dia del calendario 
            var hoy = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);

            return hoy.AddYears(-(anios ?? 0)).AddMonths(-(meses ?? 0)).AddDays(-(dias ?? 0));
        }


        //PUBLICACIONES DEL REFUGIO


        /// <summary>
        /// Leyendas del estado de la publicaicn
        /// </summary>
        /// <param name="estado"></param>
        /// <returns></returns>
        public static string EstadoPublicacion(EstadoPublicacion estado) => estado switch
        {
            Structures.EstadoPublicacion.Activa => "Activa",
            Structures.EstadoPublicacion.Pausada => "Pausada",
            Structures.EstadoPublicacion.Cerrada => "Cerrada",
            _ => string.Empty
        };

        /// <summary>
        /// Color de las leyendas del estado publicaicon
        /// </summary>
        /// <param name="estado"></param>
        /// <returns></returns>
        public static string ColorEstadoPublicacion(EstadoPublicacion estado) => estado switch
        {
            Structures.EstadoPublicacion.Activa => "bg-success",
            Structures.EstadoPublicacion.Pausada => "bg-warning text-dark",
            _ => "bg-secondary"
        };


    }
}
