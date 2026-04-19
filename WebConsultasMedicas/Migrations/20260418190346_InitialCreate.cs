using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebConsultasMedicas.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Especialidad",
                columns: table => new
                {
                    IdEspecialidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especialidad", x => x.IdEspecialidad);
                });

            migrationBuilder.CreateTable(
                name: "EstadoCita",
                columns: table => new
                {
                    IdEstadoCita = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoCita", x => x.IdEstadoCita);
                });

            migrationBuilder.CreateTable(
                name: "Medico",
                columns: table => new
                {
                    IdMedico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CMP = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Nombres = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ApellidoPaterno = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ApellidoMaterno = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    Correo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medico", x => x.IdMedico);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.IdRol);
                });

            migrationBuilder.CreateTable(
                name: "Turno",
                columns: table => new
                {
                    IdTurno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turno", x => x.IdTurno);
                });

            migrationBuilder.CreateTable(
                name: "MedicoEspecialidad",
                columns: table => new
                {
                    IdMedicoEspecialidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMedico = table.Column<int>(type: "int", nullable: false),
                    IdEspecialidad = table.Column<int>(type: "int", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Correo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ClaveHash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    IdRol = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_Usuario_Rol",
                        column: x => x.IdRol,
                        principalTable: "Rol",
                        principalColumn: "IdRol");
                });

            migrationBuilder.CreateTable(
                name: "HorarioMedico",
                columns: table => new
                {
                    IdHorarioMedico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMedico = table.Column<int>(type: "int", nullable: false),
                    IdEspecialidad = table.Column<int>(type: "int", nullable: false),
                    IdTurno = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<byte>(type: "tinyint", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: false),
                    Cupos = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    Estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorarioMedico", x => x.IdHorarioMedico);
                    table.CheckConstraint("CK_HorarioMedico_Cupos", "[Cupos] > 0");
                    table.CheckConstraint("CK_HorarioMedico_DiaSemana", "[DiaSemana] BETWEEN 1 AND 7");
                    table.ForeignKey(
                        name: "FK_HorarioMedico_Especialidad",
                        column: x => x.IdEspecialidad,
                        principalTable: "Especialidad",
                        principalColumn: "IdEspecialidad");
                    table.ForeignKey(
                        name: "FK_HorarioMedico_Medico",
                        column: x => x.IdMedico,
                        principalTable: "Medico",
                        principalColumn: "IdMedico");
                    table.ForeignKey(
                        name: "FK_HorarioMedico_Turno",
                        column: x => x.IdTurno,
                        principalTable: "Turno",
                        principalColumn: "IdTurno");
                });

            migrationBuilder.CreateTable(
                name: "Paciente",
                columns: table => new
                {
                    IdPaciente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    DNI = table.Column<string>(type: "char(8)", unicode: false, fixedLength: true, nullable: false),
                    Nombres = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ApellidoPaterno = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ApellidoMaterno = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    FechaNacimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    Sexo = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, nullable: false),
                    Telefono = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    Direccion = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    NumeroSIS = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paciente", x => x.IdPaciente);
                    table.CheckConstraint("CK_Paciente_Sexo", "[Sexo] IN ('M','F')");
                    table.ForeignKey(
                        name: "FK_Paciente_Usuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario");
                });

            migrationBuilder.CreateTable(
                name: "CitaMedica",
                columns: table => new
                {
                    IdCita = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPaciente = table.Column<int>(type: "int", nullable: false),
                    IdMedico = table.Column<int>(type: "int", nullable: false),
                    IdEspecialidad = table.Column<int>(type: "int", nullable: false),
                    IdHorarioMedico = table.Column<int>(type: "int", nullable: false),
                    IdEstadoCita = table.Column<int>(type: "int", nullable: false),
                    FechaCita = table.Column<DateOnly>(type: "date", nullable: false),
                    HoraCita = table.Column<TimeOnly>(type: "time", nullable: false),
                    MotivoConsulta = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    Observacion = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitaMedica", x => x.IdCita);
                    table.ForeignKey(
                        name: "FK_CitaMedica_Especialidad",
                        column: x => x.IdEspecialidad,
                        principalTable: "Especialidad",
                        principalColumn: "IdEspecialidad");
                    table.ForeignKey(
                        name: "FK_CitaMedica_EstadoCita",
                        column: x => x.IdEstadoCita,
                        principalTable: "EstadoCita",
                        principalColumn: "IdEstadoCita");
                    table.ForeignKey(
                        name: "FK_CitaMedica_HorarioMedico",
                        column: x => x.IdHorarioMedico,
                        principalTable: "HorarioMedico",
                        principalColumn: "IdHorarioMedico");
                    table.ForeignKey(
                        name: "FK_CitaMedica_Medico",
                        column: x => x.IdMedico,
                        principalTable: "Medico",
                        principalColumn: "IdMedico");
                    table.ForeignKey(
                        name: "FK_CitaMedica_Paciente",
                        column: x => x.IdPaciente,
                        principalTable: "Paciente",
                        principalColumn: "IdPaciente");
                });

            migrationBuilder.CreateTable(
                name: "HistorialCita",
                columns: table => new
                {
                    IdHistorial = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCita = table.Column<int>(type: "int", nullable: false),
                    IdEstadoCita = table.Column<int>(type: "int", nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    Observacion = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: true),
                    UsuarioAccion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialCita", x => x.IdHistorial);
                    table.ForeignKey(
                        name: "FK_HistorialCita_CitaMedica",
                        column: x => x.IdCita,
                        principalTable: "CitaMedica",
                        principalColumn: "IdCita");
                    table.ForeignKey(
                        name: "FK_HistorialCita_EstadoCita",
                        column: x => x.IdEstadoCita,
                        principalTable: "EstadoCita",
                        principalColumn: "IdEstadoCita");
                });

            migrationBuilder.InsertData(
                table: "Especialidad",
                columns: new[] { "IdEspecialidad", "Descripcion", "Estado", "Nombre" },
                values: new object[,]
                {
                    { 1, "Atencion general", true, "Medicina General" },
                    { 2, "Atencion de niños", true, "Pediatria" },
                    { 3, "Control prenatal y salud materna", true, "Obstetricia" },
                    { 4, "Atencion dental", true, "Odontologia" }
                });

            migrationBuilder.InsertData(
                table: "EstadoCita",
                columns: new[] { "IdEstadoCita", "Nombre" },
                values: new object[,]
                {
                    { 1, "Programada" },
                    { 2, "Atendida" },
                    { 3, "Cancelada" },
                    { 4, "Reprogramada" },
                    { 5, "No Asistio" }
                });

            migrationBuilder.InsertData(
                table: "Rol",
                columns: new[] { "IdRol", "Estado", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Administrador" },
                    { 2, true, "Paciente" }
                });

            migrationBuilder.InsertData(
                table: "Turno",
                columns: new[] { "IdTurno", "Estado", "HoraFin", "HoraInicio", "Nombre" },
                values: new object[,]
                {
                    { 1, true, new TimeOnly(12, 0, 0), new TimeOnly(8, 0, 0), "Mañana" },
                    { 2, true, new TimeOnly(18, 0, 0), new TimeOnly(14, 0, 0), "Tarde" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CitaMedica_FechaCita",
                table: "CitaMedica",
                column: "FechaCita");

            migrationBuilder.CreateIndex(
                name: "IX_CitaMedica_IdEspecialidad",
                table: "CitaMedica",
                column: "IdEspecialidad");

            migrationBuilder.CreateIndex(
                name: "IX_CitaMedica_IdEstadoCita",
                table: "CitaMedica",
                column: "IdEstadoCita");

            migrationBuilder.CreateIndex(
                name: "IX_CitaMedica_IdHorarioMedico",
                table: "CitaMedica",
                column: "IdHorarioMedico");

            migrationBuilder.CreateIndex(
                name: "IX_CitaMedica_IdMedico",
                table: "CitaMedica",
                column: "IdMedico");

            migrationBuilder.CreateIndex(
                name: "IX_CitaMedica_IdPaciente",
                table: "CitaMedica",
                column: "IdPaciente");

            migrationBuilder.CreateIndex(
                name: "IX_CitaMedicoFechaHora",
                table: "CitaMedica",
                columns: new[] { "IdMedico", "FechaCita", "HoraCita" },
                unique: true,
                filter: "[IdEstadoCita] IN (1,2,4)");

            migrationBuilder.CreateIndex(
                name: "IX_CitaPacienteFechaHora",
                table: "CitaMedica",
                columns: new[] { "IdPaciente", "FechaCita", "HoraCita" },
                unique: true,
                filter: "[IdEstadoCita] IN (1,2,4)");

            migrationBuilder.CreateIndex(
                name: "UQ_Especialidad_Nombre",
                table: "Especialidad",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_EstadoCita_Nombre",
                table: "EstadoCita",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCita_IdCita",
                table: "HistorialCita",
                column: "IdCita");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCita_IdEstadoCita",
                table: "HistorialCita",
                column: "IdEstadoCita");

            migrationBuilder.CreateIndex(
                name: "IX_HorarioMedico_IdEspecialidad",
                table: "HorarioMedico",
                column: "IdEspecialidad");

            migrationBuilder.CreateIndex(
                name: "IX_HorarioMedico_IdMedico",
                table: "HorarioMedico",
                column: "IdMedico");

            migrationBuilder.CreateIndex(
                name: "IX_HorarioMedico_IdTurno",
                table: "HorarioMedico",
                column: "IdTurno");

            migrationBuilder.CreateIndex(
                name: "UQ_Medico_CMP",
                table: "Medico",
                column: "CMP",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicoEspecialidad_IdEspecialidad",
                table: "MedicoEspecialidad",
                column: "IdEspecialidad");

            migrationBuilder.CreateIndex(
                name: "UQ_MedicoEspecialidad",
                table: "MedicoEspecialidad",
                columns: new[] { "IdMedico", "IdEspecialidad" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Paciente_DNI",
                table: "Paciente",
                column: "DNI",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Paciente_IdUsuario",
                table: "Paciente",
                column: "IdUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Paciente_NumeroSIS",
                table: "Paciente",
                column: "NumeroSIS",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Rol_Nombre",
                table: "Rol",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_IdRol",
                table: "Usuario",
                column: "IdRol");

            migrationBuilder.CreateIndex(
                name: "UQ_Usuario_Correo",
                table: "Usuario",
                column: "Correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialCita");

            migrationBuilder.DropTable(
                name: "MedicoEspecialidad");

            migrationBuilder.DropTable(
                name: "CitaMedica");

            migrationBuilder.DropTable(
                name: "EstadoCita");

            migrationBuilder.DropTable(
                name: "HorarioMedico");

            migrationBuilder.DropTable(
                name: "Paciente");

            migrationBuilder.DropTable(
                name: "Especialidad");

            migrationBuilder.DropTable(
                name: "Medico");

            migrationBuilder.DropTable(
                name: "Turno");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Rol");
        }
    }
}
