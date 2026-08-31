using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitAR.Structures.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Condiciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Condiciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Especies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Refugios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Localidad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MensajeRechazoAutomatico = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refugios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Rol = table.Column<int>(type: "int", nullable: false),
                    RolRefugio = table.Column<int>(type: "int", nullable: true),
                    RefugioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactosRefugio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RefugioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactosRefugio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactosRefugio_Refugios_RefugioId",
                        column: x => x.RefugioId,
                        principalTable: "Refugios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mascotas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RefugioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EspecieId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CondicionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Raza = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Sexo = table.Column<int>(type: "int", nullable: true),
                    FechaNacimientoAproximada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tamanio = table.Column<int>(type: "int", nullable: true),
                    Vacunado = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FotosUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaAlta = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mascotas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mascotas_Refugios_RefugioId",
                        column: x => x.RefugioId,
                        principalTable: "Refugios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerfilPostulantes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Seleccion = table.Column<int>(type: "int", nullable: false),
                    FotoUrl = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TipoVivienda = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TienePatio = table.Column<bool>(type: "bit", nullable: false),
                    PatioCerrado = table.Column<bool>(type: "bit", nullable: false),
                    TieneOtrasMascotas = table.Column<bool>(type: "bit", nullable: false),
                    DetalleOtrasMascotas = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    HorasSoloPorDia = table.Column<int>(type: "int", nullable: true),
                    CercaniaVeterinaria = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ExperienciaPrevia = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    MotivoPostulacion = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    FechaCompletado = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilPostulantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilPostulantes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Publicaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MascotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FotosUrlExtra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlazoEstimado = table.Column<int>(type: "int", nullable: true),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publicaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Publicaciones_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContactoPostulantes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerfilPostulanteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactoPostulantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactoPostulantes_PerfilPostulantes_PerfilPostulanteId",
                        column: x => x.PerfilPostulanteId,
                        principalTable: "PerfilPostulantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Postulaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    DisponibilidadFecha = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisponibilidadHorario = table.Column<int>(type: "int", nullable: true),
                    FechaPostulacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ObservacionRechazo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postulaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Postulaciones_Publicaciones_PublicacionId",
                        column: x => x.PublicacionId,
                        principalTable: "Publicaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mensajes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostulacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmisorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Leido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensajes_Postulaciones_PostulacionId",
                        column: x => x.PostulacionId,
                        principalTable: "Postulaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mensajes_Usuarios_EmisorId",
                        column: x => x.EmisorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tenencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostulacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MascotaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinEstimada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinReal = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaConversion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ObservacionCierre = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FinalizoBien = table.Column<bool>(type: "bit", nullable: true),
                    Modalidad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenencias_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tenencias_Postulaciones_PostulacionId",
                        column: x => x.PostulacionId,
                        principalTable: "Postulaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Seguimientos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenenciaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaProgramada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRealizada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seguimientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seguimientos_Tenencias_TenenciaId",
                        column: x => x.TenenciaId,
                        principalTable: "Tenencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactoPostulantes_PerfilPostulanteId_Tipo",
                table: "ContactoPostulantes",
                columns: new[] { "PerfilPostulanteId", "Tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactosRefugio_RefugioId_Tipo",
                table: "ContactosRefugio",
                columns: new[] { "RefugioId", "Tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mascotas_RefugioId",
                table: "Mascotas",
                column: "RefugioId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensajes_EmisorId",
                table: "Mensajes",
                column: "EmisorId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensajes_PostulacionId",
                table: "Mensajes",
                column: "PostulacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilPostulantes_UsuarioId",
                table: "PerfilPostulantes",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Postulaciones_PublicacionId_UsuarioId",
                table: "Postulaciones",
                columns: new[] { "PublicacionId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Publicaciones_MascotaId",
                table: "Publicaciones",
                column: "MascotaId");

            migrationBuilder.CreateIndex(
                name: "IX_Seguimientos_TenenciaId",
                table: "Seguimientos",
                column: "TenenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenencias_MascotaId",
                table: "Tenencias",
                column: "MascotaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenencias_PostulacionId",
                table: "Tenencias",
                column: "PostulacionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Condiciones");

            migrationBuilder.DropTable(
                name: "ContactoPostulantes");

            migrationBuilder.DropTable(
                name: "ContactosRefugio");

            migrationBuilder.DropTable(
                name: "Especies");

            migrationBuilder.DropTable(
                name: "Mensajes");

            migrationBuilder.DropTable(
                name: "Seguimientos");

            migrationBuilder.DropTable(
                name: "PerfilPostulantes");

            migrationBuilder.DropTable(
                name: "Tenencias");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Postulaciones");

            migrationBuilder.DropTable(
                name: "Publicaciones");

            migrationBuilder.DropTable(
                name: "Mascotas");

            migrationBuilder.DropTable(
                name: "Refugios");
        }
    }
}
