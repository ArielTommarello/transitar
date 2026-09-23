using TransitAR.Structures.Requests;
using TransitAR.Structures;

namespace TransitAR.Api.Services
{
    /// <summary>
    /// Interfaz para el manejo del perfil del refugio, suma los datos con los que se registro + los que agrega para completar el perfil
    /// </summary>
    public interface IRefugioService
    {
     /// <summary>
     /// Devuelve el perfil del refugio que hace la consulta
     /// </summary>
     /// <param name="refugioId"></param>
     /// <returns></returns>
     Task<RefugioResponse?> ObtenerPerfilAsync(Guid refugioId);

    /// <summary>
    /// Actualiza los datos del prefugio y sincroniza contactos
    /// </summary>
    /// <param name="request"></param>
    /// <param name="refugioId"></param>
    /// <returns></returns>
     Task<RefugioResponse?> ActualizarPerfilAsync(RefugioRequest request, Guid refugioId);
    }
}
