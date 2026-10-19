using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitAR.Structures.Migrations
{
    /// <inheritdoc />
    public partial class QuitarFotosUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotosUrlExtra",
                table: "Publicaciones");

            migrationBuilder.DropColumn(
                name: "FotosUrl",
                table: "Mascotas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotosUrlExtra",
                table: "Publicaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotosUrl",
                table: "Mascotas",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
