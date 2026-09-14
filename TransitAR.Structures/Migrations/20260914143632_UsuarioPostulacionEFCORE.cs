using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitAR.Structures.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioPostulacionEFCORE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Postulaciones_UsuarioId",
                table: "Postulaciones",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Postulaciones_Usuarios_UsuarioId",
                table: "Postulaciones",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Postulaciones_Usuarios_UsuarioId",
                table: "Postulaciones");

            migrationBuilder.DropIndex(
                name: "IX_Postulaciones_UsuarioId",
                table: "Postulaciones");
        }
    }
}
