using Microsoft.EntityFrameworkCore;
using TransitAR.Api.Services;
using TransitAR.Structures;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger UI for .NET
builder.Services.AddSwaggerGen();

//Context database for use in the controllers
builder.Services.AddDbContext<TransitARContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TransitAR")));

builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
