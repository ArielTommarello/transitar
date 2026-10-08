using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace TransitAR.Web.Services
{
    /// <summary>
    /// Agregamos el JWT del usuario que se loguio, y lo guardamos en la cookie.
    /// </summary>
    public class TokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _accessor;

        /// <summary>
        /// El accessor da acceso al usuario del request actual
        /// </summary>
        public TokenHandler(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        /// <summary>
        /// Antes de mandar el request agregamos el header auth para el que se haya logueado
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var contexto = _accessor.HttpContext;

            if (contexto != null)
            {
                var token = await contexto.GetTokenAsync("access_token");

                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
