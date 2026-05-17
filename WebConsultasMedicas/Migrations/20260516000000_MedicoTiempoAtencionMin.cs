using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WebConsultasMedicas.Data;

#nullable disable

namespace WebConsultasMedicas.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260516000000_MedicoTiempoAtencionMin")]
public partial class MedicoTiempoAtencionMin : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Medico','TiempoAtencionMin') IS NULL
BEGIN
    ALTER TABLE dbo.Medico ADD TiempoAtencionMin INT NOT NULL CONSTRAINT DF_Medico_TiempoAtencionMin DEFAULT(60);
END
");

        migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Medico_TiempoAtencionMin'
      AND parent_object_id = OBJECT_ID('dbo.Medico')
)
BEGIN
    ALTER TABLE dbo.Medico DROP CONSTRAINT CK_Medico_TiempoAtencionMin;
END
");

        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Medico','TiempoAtencionMin') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Medico WITH CHECK
    ADD CONSTRAINT CK_Medico_TiempoAtencionMin CHECK (TiempoAtencionMin BETWEEN 1 AND 60);
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Medico_TiempoAtencionMin'
      AND parent_object_id = OBJECT_ID('dbo.Medico')
)
BEGIN
    ALTER TABLE dbo.Medico DROP CONSTRAINT CK_Medico_TiempoAtencionMin;
END
");

        migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.default_constraints dc
    JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID('dbo.Medico')
      AND c.name = 'TiempoAtencionMin'
)
BEGIN
    DECLARE @dcName sysname;
    SELECT TOP 1 @dcName = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID('dbo.Medico')
      AND c.name = 'TiempoAtencionMin';

    EXEC('ALTER TABLE dbo.Medico DROP CONSTRAINT [' + @dcName + ']');
END
");

        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Medico','TiempoAtencionMin') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Medico DROP COLUMN TiempoAtencionMin;
END
");
    }
}

