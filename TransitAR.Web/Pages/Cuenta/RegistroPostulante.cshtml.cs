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
    /// Registro de un Postulante que uqiere adoptar o transitar (rol usuario)
    /// </summary>
    public class RegistroPostulanteModel : PageModel
    {
        private readonly ApiClient _api;

        public RegistroPostulanteModel(ApiClient api)
        {
            _api = api;
        }

        /// <summary>
        /// Datos que completea el posutalnte
        /// </summary>
        [BindProperty]
        public RegistroPostulanteRequest Datos { get; set; } = new();

        /// <summary>
        /// Mensaje de error de la API
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Mensaje luego del resgistro
        /// </summary>
        [TempData]
        public string? Mensaje { get; set; }


        //viene con la pagina
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            //respeusta en caso de exito
            var respuesta = await _api.PostAsync<object>("api/Auth/registro/postulante", Datos);

            if (!respuesta.Exito)
            {
                Error = respuesta.Error;
                return Page();
            }

            //Mensaje = "Cuenta creada. Ya podés iniciar sesión.";
            //return RedirectToPage("/Cuenta/Login");

            //autologin por una vez, asi peudo mandar a registralos
            var login = await _api.PostAsync<LoginResponse>("api/Auth/login", new LoginRequest
            {
                Email = Datos.Email,
                Password = Datos.Password
            });

            //la cuenta se creo pero no pudo entrar (inicio de sesion manual)
            if (!login.Exito || login.Datos == null)
            {
                Mensaje = "Cuenta creada. Ya podés iniciar sesión.";
                return RedirectToPage("/Cuenta/Login");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, login.Datos.UsuarioId.ToString()),
                new(ClaimTypes.Name, login.Datos.Nombre),
                new(ClaimTypes.Email, login.Datos.Email),
                new(ClaimTypes.Role, login.Datos.Rol.ToString())
            };

            var propiedades = new AuthenticationProperties
            {
                ExpiresUtc = DateTime.SpecifyKind(login.Datos.FechaExpiracion, DateTimeKind.Utc)
            };

            propiedades.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = login.Datos.Token }
            });

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
                propiedades);

            //solo si es la primera vez
            return RedirectToPage("/Postulante/Perfil");
        }
    }
}
