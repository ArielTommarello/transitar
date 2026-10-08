using System.Net;

namespace TransitAR.Web.Services
{
    /// <summary>
    /// Cliente de la API de TransitAR (como el hub)
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient _http;

        /// <summary>
        /// El HttpClient lo crea y configura desde el principio
        /// </summary>
        public ApiClient(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// GET Hacia La Api
        /// </summary>
        public async Task<ApiRespuesta<T>> GetAsync<T>(string ruta) =>
            await LeerAsync<T>(await _http.GetAsync(ruta));

        /// <summary>
        /// POST con cuerpo JSON
        /// </summary>
        public async Task<ApiRespuesta<T>> PostAsync<T>(string ruta, object? cuerpo) =>
            await LeerAsync<T>(await _http.PostAsJsonAsync(ruta, cuerpo));

        /// <summary>
        /// PUT con cuerpo JSON
        /// </summary>
        public async Task<ApiRespuesta<T>> PutAsync<T>(string ruta, object? cuerpo) =>
            await LeerAsync<T>(await _http.PutAsJsonAsync(ruta, cuerpo));

        /// <summary>
        /// PATCH con o sin cuerpo
        /// </summary>
        public async Task<ApiRespuesta<T>> PatchAsync<T>(string ruta, object? cuerpo = null) =>
            await LeerAsync<T>(await _http.PatchAsJsonAsync(ruta, cuerpo));

        /// <summary>
        /// DELETE
        /// </summary>
        public async Task<ApiRespuesta<T>> DeleteAsync<T>(string ruta) =>
            await LeerAsync<T>(await _http.DeleteAsync(ruta));

        /// <summary>
        /// Convierte la respuesta HTTP en una ApiRespuesta   
        /// </summary>
        private static async Task<ApiRespuesta<T>> LeerAsync<T>(HttpResponseMessage respuesta)
        {
            var estado = (int)respuesta.StatusCode;

            if (respuesta.IsSuccessStatusCode)
            {
                if (respuesta.StatusCode == HttpStatusCode.NoContent)
                    return new ApiRespuesta<T> { Exito = true, Estado = estado };

                return new ApiRespuesta<T>
                {
                    Exito = true,
                    Estado = estado,
                    Datos = await respuesta.Content.ReadFromJsonAsync<T>()
                };
            }

            string? mensaje = null;
            try
            {
                var error = await respuesta.Content.ReadFromJsonAsync<ErrorApi>();
                mensaje = error?.Mensaje ?? error?.Title;
            }
            catch
            {
                //si no tenia respuesta, se usa  el mensaje por defecto
            }

            return new ApiRespuesta<T>
            {
                Exito = false,
                Estado = estado,
                Error = mensaje ?? MensajePorDefecto(respuesta.StatusCode)
            };
        }

        /// <summary>
        /// Mensaje por defecto en caso de no tener uno de la API
        /// </summary>
        private static string MensajePorDefecto(HttpStatusCode codigo) => codigo switch
        {
            HttpStatusCode.Unauthorized => "Tu sesion ha vencido!.Por favor vuelve a iniciar sesion.",
            HttpStatusCode.Forbidden => "No tienes permisos para realizar esta accion.",
            HttpStatusCode.NotFound => "No hemos encontrado lo que estabas buscando, intentalo nuevamente.",
            _ => "Ocurrio un error. Por favor intenta de nuevo."
        };

        /// <summary>
        /// errrores que devuelve la Api
        /// </summary>
        private class ErrorApi
        {
            public string? Mensaje { get; set; }
            public string? Title { get; set; }
        }
    }
}
