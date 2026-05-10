namespace WebConsultasMedicas.Models.Dashboard;

public class AdminDashboardViewModel
{
    public int TotalUsuarios { get; set; }
    public int TotalPacientes { get; set; }
    public int TotalMedicos { get; set; }

    public int CitasHoy { get; set; }
    public int CitasProx7Dias { get; set; }

    public int Programadas { get; set; }
    public int Atendidas { get; set; }
    public int Canceladas { get; set; }
    public int Reprogramadas { get; set; }
    public int NoAsistio { get; set; }

    public IReadOnlyList<TopEspecialidadMetric> TopEspecialidades { get; set; } = Array.Empty<TopEspecialidadMetric>();
}

public class TopEspecialidadMetric
{
    public string Especialidad { get; set; } = string.Empty;
    public int Total { get; set; }
}

