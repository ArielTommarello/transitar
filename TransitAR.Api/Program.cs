using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.OpenApi;
using System.Text;
using TransitAR.Api.Services;
using TransitAR.Structures;
using TransitAR.Api;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger UI for .NET
//builder.Services.AddSwaggerGen();

//swagger con boton Authorize para mandar el token a endpoint protected
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegar solo el token."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
    });
});

//Datos del admin inicial
var adminInicial = builder.Configuration.GetSection("AdminInicial");

//Datros de demo solo para dev
var cargarDemo = builder.Environment.IsDevelopment();

//Context database para el controllers, el seeding lo crea el admin si no hay uno.
builder.Services.AddDbContext<TransitARContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("TransitAR"))
    .UseSeeding((context, _) =>
    {
        if (!context.Set<Usuario>().Any(u => u.Rol == RolUsuario.Admin))
        {
            var admin = AdminInicial.Armar(adminInicial);
            if (admin != null)
            {
                context.Set<Usuario>().Add(admin);
                context.SaveChanges();
            }
        }

        //datos de demo solo en dev, y si la base no tiene refugios
        if (cargarDemo && !context.Set<Refugio>().Any())
        {
            var (usuarios, refugios) = DatosDemo.Armar();
            context.AddRange(usuarios);
            context.AddRange(refugios);
            context.SaveChanges();
        }
    })
    .UseAsyncSeeding(async (context, _, cancelacion) =>
    {
        if (!await context.Set<Usuario>().AnyAsync(u => u.Rol == RolUsuario.Admin, cancelacion))
        {
            var admin = AdminInicial.Armar(adminInicial);
            if (admin != null)
            {
                context.Set<Usuario>().Add(admin);
                await context.SaveChangesAsync(cancelacion);
            }
        }

        if (cargarDemo && !await context.Set<Refugio>().AnyAsync(cancelacion))
        {
            var (usuarios, refugios) = DatosDemo.Armar();
            context.AddRange(usuarios);
            context.AddRange(refugios);
            await context.SaveChangesAsync(cancelacion);
        }
    }));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMascotaService, MascotaService>();
builder.Services.AddScoped<ICondicionService, CondicionService>();
builder.Services.AddScoped<IEspecieService, EspecieService>();
builder.Services.AddScoped<IPublicacionService, PublicacionService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
builder.Services.AddScoped<IPostulacionService, PostulacionService>();
builder.Services.AddScoped<ITenenciaService, TenenciaService>();
builder.Services.AddScoped<IRefugioService, RefugioService>();
builder.Services.AddScoped<IMensajeService, MensajeService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ISeguimientoService, SeguimientoService>();

//reviso que tenga la configuracion de la clave
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta la configuracion Jwt:Key.");

//scope para el servicio de login de jwt
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        //claims con nombre original sin mapeo 
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
