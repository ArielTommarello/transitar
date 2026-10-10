using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitAR.Structures.Migrations
{
    /// <inheritdoc />
    public partial class DisponibilidadHorariaEnPerfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisponibilidadFecha",
                table: "Postulaciones");

            migrationBuilder.DropColumn(
                name: "DisponibilidadHorario",
                table: "Postulaciones");

            migrationBuilder.AddColumn<int>(
                name: "DisponibilidadHorario",
                table: "PerfilPostulantes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisponibilidadHorario",
                table: "PerfilPostulantes");

            migrationBuilder.AddColumn<DateTime>(
                name: "DisponibilidadFecha",
                table: "Postulaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisponibilidadHorario",
                table: "Postulaciones",
                type: "int",
                nullable: true);
        }
    }
}
