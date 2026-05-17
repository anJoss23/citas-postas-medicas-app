using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebConsultasMedicas.Models.Reportes;

public class AtencionesPorMedicoFilterViewModel
{
    public int? IdEspecialidad { get; set; }
    public int? IdMedico { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }

    public SelectList? Especialidades { get; set; }
    public SelectList? Medicos { get; set; }

    public IReadOnlyList<AtencionPorMedicoRow> Rows { get; set; } = Array.Empty<AtencionPorMedicoRow>();
}

public class AtencionPorMedicoRow
{
    public int IdCita { get; set; }
    public DateOnly FechaCita { get; set; }
    public TimeOnly HoraCita { get; set; }

    public string Especialidad { get; set; } = string.Empty;
    public string Medico { get; set; } = string.Empty;
    public string Paciente { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;
    public string? UltimaObservacion { get; set; }
    public DateTime? UltimoCambio { get; set; }
}
