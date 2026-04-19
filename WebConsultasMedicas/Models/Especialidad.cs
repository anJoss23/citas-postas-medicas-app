namespace WebConsultasMedicas.Models;

using System.ComponentModel.DataAnnotations;

public class Especialidad
{
    public int IdEspecialidad { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no debe exceder 100 caracteres.")]
    public string Nombre { get; set; } = null!;

    [StringLength(200, ErrorMessage = "La descripción no debe exceder 200 caracteres.")]
    public string? Descripcion { get; set; }
    public bool Estado { get; set; } = true;

    public ICollection<Medico> Medicos { get; set; } = new List<Medico>();
    public ICollection<HorarioMedico> Horarios { get; set; } = new List<HorarioMedico>();
    public ICollection<CitaMedica> Citas { get; set; } = new List<CitaMedica>();
}
