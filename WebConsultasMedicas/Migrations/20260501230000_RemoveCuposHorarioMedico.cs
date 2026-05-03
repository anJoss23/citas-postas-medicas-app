using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WebConsultasMedicas.Data;

#nullable disable

namespace WebConsultasMedicas.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260501230000_RemoveCuposHorarioMedico")]
public partial class RemoveCuposHorarioMedico : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Make it idempotent: drop constraint and column only if they exist.
        migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_HorarioMedico_Cupos'
      AND parent_object_id = OBJECT_ID('dbo.HorarioMedico')
)
BEGIN
    ALTER TABLE dbo.HorarioMedico DROP CONSTRAINT CK_HorarioMedico_Cupos;
END
");

        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.HorarioMedico','Cupos') IS NOT NULL
BEGIN
    DECLARE @dfName sysname;
    SELECT @dfName = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID('dbo.HorarioMedico')
      AND c.name = 'Cupos';

    IF @dfName IS NOT NULL
    BEGIN
        DECLARE @sql NVARCHAR(MAX) = N'ALTER TABLE dbo.HorarioMedico DROP CONSTRAINT [' + @dfName + N'];';
        EXEC(@sql);
    END

    ALTER TABLE dbo.HorarioMedico DROP COLUMN Cupos;
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Recreate column only if missing. (Defaults/constraints recreated best-effort.)
        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.HorarioMedico','Cupos') IS NULL
BEGIN
    ALTER TABLE dbo.HorarioMedico ADD Cupos INT NOT NULL CONSTRAINT DF_HorarioMedico_Cupos DEFAULT(10);
    ALTER TABLE dbo.HorarioMedico WITH CHECK ADD CONSTRAINT CK_HorarioMedico_Cupos CHECK (Cupos > 0);
END
");
    }
}
