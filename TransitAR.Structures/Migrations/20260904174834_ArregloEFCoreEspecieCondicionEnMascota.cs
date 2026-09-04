using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitAR.Structures.Migrations
{
    /// <inheritdoc />
    public partial class ArregloEFCoreEspecieCondicionEnMascota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Mascotas_CondicionId",
                table: "Mascotas",
                column: "CondicionId");

            migrationBuilder.CreateIndex(
                name: "IX_Mascotas_EspecieId",
                table: "Mascotas",
                column: "EspecieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mascotas_Condiciones_CondicionId",
                table: "Mascotas",
                column: "CondicionId",
                principalTable: "Condiciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mascotas_Especies_EspecieId",
                table: "Mascotas",
                column: "EspecieId",
                principalTable: "Especies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mascotas_Condiciones_CondicionId",
                table: "Mascotas");

            migrationBuilder.DropForeignKey(
                name: "FK_Mascotas_Especies_EspecieId",
                table: "Mascotas");

            migrationBuilder.DropIndex(
                name: "IX_Mascotas_CondicionId",
                table: "Mascotas");

            migrationBuilder.DropIndex(
                name: "IX_Mascotas_EspecieId",
                table: "Mascotas");
        }
    }
}
