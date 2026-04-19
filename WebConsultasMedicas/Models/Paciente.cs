namespace WebConsultasMedicas.Models;

using System.ComponentModel.DataAnnotations;

public class Paciente
{
    public int IdPaciente { get; set; }

    [Required]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos.")]
    public string DNI { get; set; } = null!;

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100, ErrorMessage = "Los nombres no deben exceder 100 caracteres.")]
    public string Nombres { get; set; } = null!;

    [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
    [StringLength(100, ErrorMessage = "El apellido paterno no debe exceder 100 caracteres.")]
    public string ApellidoPaterno { get; set; } = null!;

    [Required(ErrorMessage = "El apellido materno es obligatorio.")]
    [StringLength(100, ErrorMessage = "El apellido materno no debe exceder 100 caracteres.")]
    public string ApellidoMaterno { get; set; } = null!;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateOnly FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El sexo es obligatorio.")]
    [StringLength(1, MinimumLength = 1)]
    public string Sexo { get; set; } = null!;

    [StringLength(15, ErrorMessage = "El teléfono no debe exceder 15 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no debe exceder 200 caracteres.")]
    public string? Direccion { get; set; }

    [Required(ErrorMessage = "El número SIS es obligatorio.")]
    [StringLength(20, ErrorMessage = "El número SIS no debe exceder 20 caracteres.")]
    public string NumeroSIS { get; set; } = null!;

    public bool Estado { get; set; } = true;

    public Usuario Usuario { get; set; } = null!;
    public ICollection<CitaMedica> Citas { get; set; } = new List<CitaMedica>();
}
