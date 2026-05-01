using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebConsultasMedicas.Migrations
{
    /// <inheritdoc />
    public partial class RolesMedicoYVinculoUsuarioMedico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: This migration was originally scaffolded as standard AddColumn/CreateIndex/AddForeignKey.
            // In some dev environments the column may have been added manually, so we make it idempotent.
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Medico','IdUsuario') IS NULL
BEGIN
    ALTER TABLE dbo.Medico ADD IdUsuario INT NULL;
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UQ_Medico_IdUsuario'
      AND object_id = OBJECT_ID('dbo.Medico')
)
BEGIN
    CREATE UNIQUE INDEX UQ_Medico_IdUsuario ON dbo.Medico(IdUsuario)
    WHERE IdUsuario IS NOT NULL;
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Medico_Usuario'
      AND parent_object_id = OBJECT_ID('dbo.Medico')
)
BEGIN
    ALTER TABLE dbo.Medico WITH CHECK
    ADD CONSTRAINT FK_Medico_Usuario FOREIGN KEY (IdUsuario)
    REFERENCES dbo.Usuario (IdUsuario);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Rol WHERE Nombre = 'Medico')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID('dbo.Rol') AND name = 'IdRol')
    BEGIN
        SET IDENTITY_INSERT dbo.Rol ON;
        INSERT INTO dbo.Rol (IdRol, Nombre, Estado) VALUES (3, 'Medico', 1);
        SET IDENTITY_INSERT dbo.Rol OFF;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Rol (Nombre, Estado) VALUES ('Medico', 1);
    END
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Medico_Usuario'
      AND parent_object_id = OBJECT_ID('dbo.Medico')
)
BEGIN
    ALTER TABLE dbo.Medico DROP CONSTRAINT FK_Medico_Usuario;
END
");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UQ_Medico_IdUsuario'
      AND object_id = OBJECT_ID('dbo.Medico')
)
BEGIN
    DROP INDEX UQ_Medico_IdUsuario ON dbo.Medico;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Medico','IdUsuario') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Medico DROP COLUMN IdUsuario;
END
");
        }
    }
}
