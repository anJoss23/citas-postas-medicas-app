using System.ComponentModel.DataAnnotations;

namespace WebConsultasMedicas.Models;

public class PatientRegisterViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    [StringLength(100)]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos.")]
    public string DNI { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
    [StringLength(100)]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido materno es obligatorio.")]
    [StringLength(100)]
    public string ApellidoMaterno { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateOnly FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El sexo es obligatorio.")]
    [RegularExpression("^[MF]$", ErrorMessage = "Sexo inválido (M/F).")]
    public string Sexo { get; set; } = "M";

    [StringLength(15)]
    public string? Telefono { get; set; }

    [StringLength(200)]
    public string? Direccion { get; set; }

    [Required(ErrorMessage = "El número SIS es obligatorio.")]
    [StringLength(20)]
    public string NumeroSIS { get; set; } = string.Empty;
}

