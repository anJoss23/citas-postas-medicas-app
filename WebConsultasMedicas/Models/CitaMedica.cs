namespace WebConsultasMedicas.Models;

using System.ComponentModel.DataAnnotations;

public class CitaMedica
{
    public int IdCita { get; set; }

    [Required]
    public int IdPaciente { get; set; }

    [Required]
    public int IdMedico { get; set; }

    [Required]
    public int IdEspecialidad { get; set; }

    [Required]
    public int IdHorarioMedico { get; set; }

    [Required]
    public int IdEstadoCita { get; set; }

    [Required]
    public DateOnly FechaCita { get; set; }

    [Required]
    public TimeOnly HoraCita { get; set; }

    [StringLength(250, ErrorMessage = "El motivo no debe exceder 250 caracteres.")]
    public string? MotivoConsulta { get; set; }

    [StringLength(250, ErrorMessage = "La observación no debe exceder 250 caracteres.")]
    public string? Observacion { get; set; }

    public DateTime FechaRegistro { get; set; }

    public Paciente Paciente { get; set; } = null!;
    public Medico Medico { get; set; } = null!;
    public Especialidad Especialidad { get; set; } = null!;
    public HorarioMedico HorarioMedico { get; set; } = null!;
    public EstadoCita EstadoCita { get; set; } = null!;
    public ICollection<HistorialCita> Historial { get; set; } = new List<HistorialCita>();
}
