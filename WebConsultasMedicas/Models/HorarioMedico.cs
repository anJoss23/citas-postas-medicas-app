namespace WebConsultasMedicas.Models;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

    [ValidateNever]
    public Medico Medico { get; set; } = null!;
    [ValidateNever]
    public Especialidad Especialidad { get; set; } = null!;
    [ValidateNever]
    public Turno Turno { get; set; } = null!;
    [ValidateNever]
    public ICollection<CitaMedica> Citas { get; set; } = new List<CitaMedica>();
}
