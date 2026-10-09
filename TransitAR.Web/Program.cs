using Microsoft.AspNetCore.Authentication.Cookies;
using TransitAR.Structures;
using TransitAR.Web.Services;

var builder = WebApplication.CreateBuilder(args);


//dividido por rol. paa poder manejar los menues y roles 
builder.Services.AddRazorPages(opciones =>
{
    opciones.Conventions.AuthorizeFolder("/Postulante", "Postulante");
    opciones.Conventions.AuthorizeFolder("/Refugio", "Refugio");
    opciones.Conventions.AuthorizeFolder("/Admin", "Admin");
    opciones.Conventions.AuthorizeFolder("/Mensajes");
});

//autorizaciones de las carpetas que uso para los roles, el rol lo dejo en la cookie
builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy("Postulante", p => p.RequireRole(nameof(RolUsuario.Usuario)));
    opciones.AddPolicy("Refugio", p => p.RequireRole(nameof(RolUsuario.Refugio)));
    opciones.AddPolicy("Admin", p => p.RequireRole(nameof(RolUsuario.Admin)));
});

builder.Services.AddHttpContextAccessor();

//cliente de la API 
builder.Services.AddTransient<TokenHandler>();
builder.Services.AddHttpClient<ApiClient>(cliente =>
{
    cliente.BaseAddress = new Uri(builder.Configuration["Api:UrlBase"]!);
})
.AddHttpMessageHandler<TokenHandler>();

//sesion del usuaior, guardamos el jwt en la cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Cuenta/Login";
        opciones.LogoutPath = "/Cuenta/Logout";
        opciones.AccessDeniedPath = "/Cuenta/AccesoDenegado";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        opciones.Cookie.SameSite = SameSiteMode.Lax;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

//authentications - authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();