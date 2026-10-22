namespace TransitAR.Api.Extensions
{
    /// <summary>
    ///fechas usadas en los DTOs de salida
    /// </summary>
    public  static class FechaExtensions
    {

        /// <summary>
        /// Calcula la edad en meses a partir del dia de nacimiento, contra el dia actual. Null si no se sabe
        /// </summary>
        public static int? EdadEnMeses(this DateTime? nacimiento)
        {
            if (nacimiento == null)
                return null;

            var hoy = DateTime.UtcNow;
            var meses = ((hoy.Year - nacimiento.Value.Year) * 12) + hoy.Month - nacimiento.Value.Month;

            if (hoy.Day < nacimiento.Value.Day)
                meses--;

            return meses < 0 ? 0 : meses;
        }

        /// <summary>
        /// Calcula la edad en dias a partir del dia de nacimiento, contra el dia actual. Null si no se sabe
        /// </summary>
        public static int? EdadEnDias(this DateTime? nacimiento)
        {
            if (nacimiento == null)
                return null;

            var dias = (int)(DateTime.UtcNow.Date - nacimiento.Value.Date).TotalDays;
            return dias < 0 ? 0 : dias;
        }
    }
}
