using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using WebConsultasMedicas.Data;

#nullable disable

namespace WebConsultasMedicas.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260501231000_HorarioFechaEnLugarDeDiaSemana")]
public partial class HorarioFechaEnLugarDeDiaSemana : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1) Add Fecha if missing (nullable first).
        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.HorarioMedico','Fecha') IS NULL
BEGIN
    ALTER TABLE dbo.HorarioMedico ADD Fecha DATE NULL;
END
");

        // 2) Backfill Fecha for existing rows if still NULL.
        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.HorarioMedico','Fecha') IS NOT NULL
BEGIN
    UPDATE dbo.HorarioMedico
    SET Fecha = CONVERT(date, GETDATE())
    WHERE Fecha IS NULL;
END
");

        // 3) Make Fecha NOT NULL (idempotent check via sys.columns).
        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.HorarioMedico','Fecha') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID('dbo.HorarioMedico')
          AND name = 'Fecha'
          AND is_nullable = 1
    )
    BEGIN
        ALTER TABLE dbo.HorarioMedico ALTER COLUMN Fecha DATE NOT NULL;
    END
END
");

        // 4) Drop DiaSemana constraint and column if present.
        migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_HorarioMedico_DiaSemana'
      AND parent_object_id = OBJECT_ID('dbo.HorarioMedico')
)
BEGIN
    ALTER TABLE dbo.HorarioMedico DROP CONSTRAINT CK_HorarioMedico_DiaSemana;
END
");

        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.HorarioMedico','DiaSemana') IS NOT NULL
BEGIN
    ALTER TABLE dbo.HorarioMedico DROP COLUMN DiaSemana;
END
");

        // 5) Add index on Fecha if missing.
        migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_HorarioMedico_Fecha'
      AND object_id = OBJECT_ID('dbo.HorarioMedico')
)
BEGIN
    CREATE INDEX IX_HorarioMedico_Fecha ON dbo.HorarioMedico(Fecha);
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Best-effort rollback: add DiaSemana back (default to 1), drop Fecha.
        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.HorarioMedico','DiaSemana') IS NULL
BEGIN
    ALTER TABLE dbo.HorarioMedico ADD DiaSemana TINYINT NOT NULL CONSTRAINT DF_HorarioMedico_DiaSemana DEFAULT(1);
    ALTER TABLE dbo.HorarioMedico WITH CHECK ADD CONSTRAINT CK_HorarioMedico_DiaSemana CHECK (DiaSemana BETWEEN 1 AND 7);
END
");

        migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_HorarioMedico_Fecha'
      AND object_id = OBJECT_ID('dbo.HorarioMedico')
)
BEGIN
    DROP INDEX IX_HorarioMedico_Fecha ON dbo.HorarioMedico;
END
");

        migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.HorarioMedico','Fecha') IS NOT NULL
BEGIN
    ALTER TABLE dbo.HorarioMedico DROP COLUMN Fecha;
END
");
    }
}
