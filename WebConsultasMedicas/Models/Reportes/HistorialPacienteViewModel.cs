using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebConsultasMedicas.Models.Reportes;

public class HistorialPacienteViewModel
{
    public int? IdPaciente { get; set; }
    public DateOnly? FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public SelectList? Pacientes { get; set; }

    public IReadOnlyList<HistorialPacienteRow> Rows { get; set; } = Array.Empty<HistorialPacienteRow>();
}

public class HistorialPacienteRow
{
    public int IdCita { get; set; }
    public DateOnly FechaCita { get; set; }
    public TimeOnly HoraCita { get; set; }

    public string Especialidad { get; set; } = string.Empty;
    public string Medico { get; set; } = string.Empty;

    public DateTime FechaCambio { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public string? UsuarioAccion { get; set; }
}
