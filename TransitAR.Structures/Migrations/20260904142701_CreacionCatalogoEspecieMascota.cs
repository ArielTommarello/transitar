using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TransitAR.Structures.Migrations
{
    /// <inheritdoc />
    public partial class CreacionCatalogoEspecieMascota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Condiciones",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222201"), null, "Sano" },
                    { new Guid("22222222-2222-2222-2222-222222222202"), null, "En tratamiento" },
                    { new Guid("22222222-2222-2222-2222-222222222203"), null, "Discapacidad permanente" }
                });

            migrationBuilder.InsertData(
                table: "Especies",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), null, "Perro" },
                    { new Guid("11111111-1111-1111-1111-111111111102"), null, "Gato" },
                    { new Guid("11111111-1111-1111-1111-111111111103"), null, "Conejo" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Condiciones",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222201"));

            migrationBuilder.DeleteData(
                table: "Condiciones",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222202"));

            migrationBuilder.DeleteData(
                table: "Condiciones",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222203"));

            migrationBuilder.DeleteData(
                table: "Especies",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111101"));

            migrationBuilder.DeleteData(
                table: "Especies",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111102"));

            migrationBuilder.DeleteData(
                table: "Especies",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111103"));
        }
    }
}
