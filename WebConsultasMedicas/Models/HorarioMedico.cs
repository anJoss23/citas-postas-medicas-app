namespace WebConsultasMedicas.Models;

using System.ComponentModel.DataAnnotations;

public class HorarioMedico
{
    public int IdHorarioMedico { get; set; }

    [Required]
    public int IdMedico { get; set; }

    [Required]
    public int IdEspecialidad { get; set; }

    [Required]
    public int IdTurno { get; set; }

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateOnly Fecha { get; set; }

    [Required]
    public TimeOnly HoraInicio { get; set; }

    [Required]
    public TimeOnly HoraFin { get; set; }

    public bool Estado { get; set; } = true;

    public Medico Medico { get; set; } = null!;
    public Especialidad Especialidad { get; set; } = null!;
    public Turno Turno { get; set; } = null!;
    public ICollection<CitaMedica> Citas { get; set; } = new List<CitaMedica>();
}

