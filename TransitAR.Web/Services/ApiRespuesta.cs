namespace TransitAR.Web.Services
{
    /// <summary>
    /// Resultado de la llamada a la api , si falla trae un mensaej
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiRespuesta<T>
    {
        /// <summary>
        /// codigo de exito de la respuesta
        /// </summary>
        public bool Exito { get; set; }

        /// <summary>
        /// Los datos de la respuesta null si fallo
        /// </summary>
        public T? Datos { get; set; }

        /// <summary>
        /// El mensaje de error de la API
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// El codigo HTTP para ver entre 401 o 400
        /// </summary>
        public int Estado { get; set; }
    }
}
