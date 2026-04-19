using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebConsultasMedicas.Migrations
{
    /// <inheritdoc />
    public partial class MedicoUnicaEspecialidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdEspecialidad",
                table: "Medico",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE M
SET M.IdEspecialidad = X.IdEspecialidad
FROM Medico AS M
OUTER APPLY (
    SELECT TOP (1) ME.IdEspecialidad
    FROM MedicoEspecialidad AS ME
    WHERE ME.IdMedico = M.IdMedico
    ORDER BY ME.IdEspecialidad
) AS X
WHERE M.IdEspecialidad IS NULL;");

            migrationBuilder.Sql(@"
UPDATE Medico
SET IdEspecialidad = 1
WHERE IdEspecialidad IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "IdEspecialidad",
                table: "Medico",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medico_IdEspecialidad",
                table: "Medico",
                column: "IdEspecialidad");

            migrationBuilder.AddForeignKey(
                name: "FK_Medico_Especialidad",
                table: "Medico",
                column: "IdEspecialidad",
                principalTable: "Especialidad",
                principalColumn: "IdEspecialidad");

            migrationBuilder.DropTable(
                name: "MedicoEspecialidad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicoEspecialidad",
                columns: table => new
                {
                    IdMedicoEspecialidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEspecialidad = table.Column<int>(type: "int", nullable: false),
                    IdMedico = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicoEspecialidad", x => x.IdMedicoEspecialidad);
                    table.ForeignKey(
                        name: "FK_MedicoEspecialidad_Especialidad",
                        column: x => x.IdEspecialidad,
                        principalTable: "Especialidad",
                        principalColumn: "IdEspecialidad");
                    table.ForeignKey(
                        name: "FK_MedicoEspecialidad_Medico",
                        column: x => x.IdMedico,
                        principalTable: "Medico",
                        principalColumn: "IdMedico");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicoEspecialidad_IdEspecialidad",
                table: "MedicoEspecialidad",
                column: "IdEspecialidad");

            migrationBuilder.CreateIndex(
                name: "UQ_MedicoEspecialidad",
                table: "MedicoEspecialidad",
                columns: new[] { "IdMedico", "IdEspecialidad" },
                unique: true);

            migrationBuilder.DropForeignKey(
                name: "FK_Medico_Especialidad",
                table: "Medico");

            migrationBuilder.DropIndex(
                name: "IX_Medico_IdEspecialidad",
                table: "Medico");

            migrationBuilder.DropColumn(
                name: "IdEspecialidad",
                table: "Medico");
        }
    }
}
