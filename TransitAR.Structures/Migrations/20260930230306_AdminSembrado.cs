using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransitAR.Structures.Migrations
{
    /// <inheritdoc />
    public partial class AdminSembrado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "Apellido", "Email", "FechaAlta", "Nombre", "PasswordHash", "RefugioId", "Rol", "RolRefugio", "Telefono" },
                values: new object[] { new Guid("99999999-9999-9999-9999-999999999901"), true, "TransitAR", "admin@transitar.org", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrador", "$2a$11$tAW0hYZZe7.WzbuJyz7WAuYyM.TVAcKXeWohP07LjbQGv7Yh6wMi.", null, 1, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999901"));
        }
    }
}
