namespace WebConsultasMedicas.Models;

using System.ComponentModel.DataAnnotations;

public class Turno
{
    public int IdTurno { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no debe exceder 50 caracteres.")]
    public string Nombre { get; set; } = null!;

    [Required]
    public TimeOnly HoraInicio { get; set; }

    [Required]
    public TimeOnly HoraFin { get; set; }

    public bool Estado { get; set; } = true;

    public ICollection<HorarioMedico> Horarios { get; set; } = new List<HorarioMedico>();
}
