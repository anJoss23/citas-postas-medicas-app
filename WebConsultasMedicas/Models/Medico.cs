namespace WebConsultasMedicas.Models;

using System.ComponentModel.DataAnnotations;

public class Medico
{
    public int IdMedico { get; set; }

    [Required]
    public int IdEspecialidad { get; set; }

    [Required(ErrorMessage = "El CMP es obligatorio.")]
    [StringLength(20, ErrorMessage = "El CMP no debe exceder 20 caracteres.")]
    public string CMP { get; set; } = null!;

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los nombres no deben exceder 100 caracteres.")]
    public string Nombres { get; set; } = null!;

    [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
    [StringLength(100, ErrorMessage = "El apellido paterno no debe exceder 100 caracteres.")]
    public string ApellidoPaterno { get; set; } = null!;

    [Required(ErrorMessage = "El apellido materno es obligatorio.")]
    [StringLength(100, ErrorMessage = "El apellido materno no debe exceder 100 caracteres.")]
    public string ApellidoMaterno { get; set; } = null!;

    [StringLength(15, ErrorMessage = "El teléfono no debe exceder 15 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(100, ErrorMessage = "El correo no debe exceder 100 caracteres.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    public string? Correo { get; set; }

    public bool Estado { get; set; } = true;

    public Especialidad Especialidad { get; set; } = null!;
    public ICollection<HorarioMedico> Horarios { get; set; } = new List<HorarioMedico>();
    public ICollection<CitaMedica> Citas { get; set; } = new List<CitaMedica>();
}
