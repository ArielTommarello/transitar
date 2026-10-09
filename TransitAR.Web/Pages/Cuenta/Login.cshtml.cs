using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using TransitAR.Structures;
using TransitAR.Web.Services;

namespace TransitAR.Web.Pages.Cuenta
{
    /// <summary>
    /// Logind e los usuarios, pido token a la api y lo guardo en la cookie de la web. Asi armo el menu
    /// </summary>
    public class LoginModel : PageModel
    {



        private readonly ApiClient _api;

        public LoginModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Datos que completa la persona
        /// </summary>
        [BindProperty]
        public LoginRequest Datos { get; set; } = new();

        /// <summary>
        /// Mensaje de error de la API
        /// </summary>
        public string? Error { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl)
        {
            if (!ModelState.IsValid)
                return Page();

            var respuesta = await _api.PostAsync<LoginResponse>("api/Auth/login", Datos);

            if (!respuesta.Exito || respuesta.Datos == null)
            {
                Error = respuesta.Error;
                return Page();
            }

            var login = respuesta.Datos;

            //dejo los datos en la cookie para armar el menu
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, login.UsuarioId.ToString()),
                new(ClaimTypes.Name, login.Nombre),
                new(ClaimTypes.Email, login.Email),
                new(ClaimTypes.Role, login.Rol.ToString())
            };

            if (login.RefugioId != null)
                claims.Add(new Claim("refugioId", login.RefugioId.Value.ToString()));

            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var propiedades = new AuthenticationProperties
            {
                //la sesion de la web vence junto con el token. La API manda la fecha en UTC 
                ExpiresUtc = DateTime.SpecifyKind(login.FechaExpiracion, DateTimeKind.Utc)
            };

            //el JWT se guarda dentro de la cookie . Nuestro TokenHandler lo obtiene con GetTokenAsync("access_token") asi lo podemos guardar
            propiedades.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = login.Token }
            });

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad),
                propiedades);

            //ssolo redirecciones locales 
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToPage("/Index");
        }
    }
}
