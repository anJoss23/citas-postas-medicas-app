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

    [Range(1, 7, ErrorMessage = "El día de semana debe estar entre 1 y 7.")]
    public byte DiaSemana { get; set; }

    [Required]
    public TimeOnly HoraInicio { get; set; }

    [Required]
    public TimeOnly HoraFin { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Los cupos deben ser mayor a 0.")]
    public int Cupos { get; set; } = 10;

    public bool Estado { get; set; } = true;

    public Medico Medico { get; set; } = null!;
    public Especialidad Especialidad { get; set; } = null!;
    public Turno Turno { get; set; } = null!;
    public ICollection<CitaMedica> Citas { get; set; } = new List<CitaMedica>();
}
