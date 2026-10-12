using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitAR.Structures.Migrations
{
    /// <inheritdoc />
    public partial class FotoPerfilBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Refugios");

            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "PerfilPostulantes");

            migrationBuilder.AddColumn<string>(
                name: "FotoContentType",
                table: "Refugios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FotoPerfil",
                table: "Refugios",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoContentType",
                table: "PerfilPostulantes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FotoPerfil",
                table: "PerfilPostulantes",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoContentType",
                table: "Refugios");

            migrationBuilder.DropColumn(
                name: "FotoPerfil",
                table: "Refugios");

            migrationBuilder.DropColumn(
                name: "FotoContentType",
                table: "PerfilPostulantes");

            migrationBuilder.DropColumn(
                name: "FotoPerfil",
                table: "PerfilPostulantes");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Refugios",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "PerfilPostulantes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }
    }
}
