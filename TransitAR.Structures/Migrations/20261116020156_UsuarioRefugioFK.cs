using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitAR.Structures.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioRefugioFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RefugioId",
                table: "Usuarios",
                column: "RefugioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Refugios_RefugioId",
                table: "Usuarios",
                column: "RefugioId",
                principalTable: "Refugios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Refugios_RefugioId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_RefugioId",
                table: "Usuarios");
        }
    }
}
