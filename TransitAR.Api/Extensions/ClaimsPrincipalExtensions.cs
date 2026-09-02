using System.Runtime.CompilerServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TransitAR.Api.Extensions
{
    /// <summary>
    /// Metodos para leer los datos desde el token 
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Nombre del claim dodne esta el refugio/usuario
        /// </summary>
        public const string ClaimRefugioId = "refugioId";


        /// <summary>
        /// id del usuario autenticado sacado del claim
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static Guid? ObtenerUsuarioId(this ClaimsPrincipal user)
        {
            var valor = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(valor, out var id) ? id : null;
        }

        /// <summary>
        /// Refugio del usuario. Null es en caso de postulante.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static Guid? ObtenerRefugioId(this ClaimsPrincipal user)
        {

            var valor = user.FindFirst(ClaimRefugioId)?.Value;
            return Guid.TryParse(valor, out var id) ? id : null;
        }

    }
}
