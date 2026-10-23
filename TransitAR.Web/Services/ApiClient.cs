using System.Net;
using System.Net.Http.Headers;

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
        /// Fotos de perfil, put con el arvhico en IFORMFILE
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruta"></param>
        /// <param name="archivo"></param>
        /// <param name="nombreCampo"></param>
        /// <returns></returns>
        public async Task<ApiRespuesta<T>> PutArchivoAsync<T>(string ruta, IFormFile archivo, string nombreCampo)
        {
            using var contenido = new MultipartFormDataContent();
            using var stream = archivo.OpenReadStream();

            var parte = new StreamContent(stream);
            parte.Headers.ContentType = new MediaTypeHeaderValue(archivo.ContentType);
            contenido.Add(parte, nombreCampo, archivo.FileName);

            return await LeerAsync<T>(await _http.PutAsync(ruta, contenido));
        }

        /// <summary>
        /// Varios archivos para subir las fotos de una mascota (POST)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruta"></param>
        /// <param name="archivos"></param>
        /// <param name="nombreCampo"></param>
        /// <returns></returns>
        public async Task<ApiRespuesta<T>> PostArchivosAsync<T>(string ruta, List<IFormFile> archivos, string nombreCampo)
        {
            using var contenido = new MultipartFormDataContent();

            //archivo por archivo para armar la lsita
            foreach (var archivo in archivos)
            {
                var parte = new StreamContent(archivo.OpenReadStream());
                parte.Headers.ContentType = new MediaTypeHeaderValue(archivo.ContentType);
                contenido.Add(parte, nombreCampo, archivo.FileName);
            }

            return await LeerAsync<T>(await _http.PostAsync(ruta, contenido));
        }


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
        /// Direccion completa de la foto de un postulante (conexion a la api por una imagne , forma standard)
        /// </summary>
        public string UrlFotoPostulante(Guid usuarioId) =>
            new Uri(_http.BaseAddress!, $"api/Perfil/{usuarioId}/foto").ToString();

        /// <summary>
        /// Direccion completa del logo de un refugio (conexion a la api por una imagne , forma standard)
        /// </summary>
        public string UrlFotoRefugio(Guid refugioId) =>
            new Uri(_http.BaseAddress!, $"api/Refugio/{refugioId}/foto").ToString();

        /// <summary>
        /// Direcciones compltras de las fotos de la masctoa con orden en que vienen
        /// </summary>
        /// <param name="fotosIds"></param>
        /// <returns></returns>
        public List<string> UrlsFotosMascota(List<Guid> fotosIds) =>
            fotosIds.Select(id => new Uri(_http.BaseAddress!, $"api/Mascota/fotos/{id}").ToString()).ToList();


        /// <summary>
        /// Direcion de la foto que es la principal y se mostrara en la tarjeta
        /// </summary>
        /// <param name="fotosIds"></param>
        /// <returns></returns>
        public string? UrlFotoPrincipal(List<Guid> fotosIds) =>
           fotosIds.Count == 0 ? null : UrlsFotosMascota(fotosIds)[0];

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
