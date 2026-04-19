using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebConsultasMedicas.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuario",
                columns: new[] { "IdUsuario", "ClaveHash", "Correo", "Estado", "FechaRegistro", "IdRol" },
                values: new object[] { 1, "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9", "admin@siscitasweb.local", true, new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "IdUsuario",
                keyValue: 1);
        }
    }
}
