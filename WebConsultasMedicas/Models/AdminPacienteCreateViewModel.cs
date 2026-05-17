using System.ComponentModel.DataAnnotations;

namespace WebConsultasMedicas.Models;

public class AdminPacienteCreateViewModel
{
    [Display(Name = "Correo")]
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    [StringLength(100)]
    public string Correo { get; set; } = string.Empty;

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Repetir contraseña")]
    [Required(ErrorMessage = "Confirma la contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "DNI")]
    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos.")]
    public string DNI { get; set; } = string.Empty;

    [Display(Name = "Nombres")]
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Display(Name = "Apellido Paterno")]
    [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
    [StringLength(100)]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [Display(Name = "Apellido Materno")]
    [Required(ErrorMessage = "El apellido materno es obligatorio.")]
    [StringLength(100)]
    public string ApellidoMaterno { get; set; } = string.Empty;

    [Display(Name = "Fecha nacimiento")]
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateOnly FechaNacimiento { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(-18));

    [Display(Name = "Sexo")]
    [Required(ErrorMessage = "El sexo es obligatorio.")]
    [RegularExpression("^[MF]$", ErrorMessage = "Sexo inválido (M/F).")]
    public string Sexo { get; set; } = "M";

    [Display(Name = "Teléfono")]
    [StringLength(15)]
    public string? Telefono { get; set; }

    [Display(Name = "Dirección")]
    [StringLength(200)]
    public string? Direccion { get; set; }

    public bool Estado { get; set; } = true;
}
