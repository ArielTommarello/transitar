using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitAR.Structures.Migrations
{
    /// <inheritdoc />
    public partial class MensajeFechaLectura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Leido",
                table: "Mensajes");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaLectura",
                table: "Mensajes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaLectura",
                table: "Mensajes");

            migrationBuilder.AddColumn<bool>(
                name: "Leido",
                table: "Mensajes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
