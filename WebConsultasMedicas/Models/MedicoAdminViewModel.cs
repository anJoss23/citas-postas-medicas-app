using System.ComponentModel.DataAnnotations;

namespace WebConsultasMedicas.Models;

public class MedicoAdminViewModel : IValidatableObject
{
    public int IdMedico { get; set; }

    [Display(Name = "Especialidad")]
    [Required]
    public int IdEspecialidad { get; set; }

    [Display(Name = "CMP")]
    [Required]
    [StringLength(20)]
    public string CMP { get; set; } = string.Empty;

    [Display(Name = "Nombres")]
    [Required]
    [StringLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Display(Name = "Apellido Paterno")]
    [Required]
    [StringLength(100)]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [Display(Name = "Apellido Materno")]
    [Required]
    [StringLength(100)]
    public string ApellidoMaterno { get; set; } = string.Empty;

    [Display(Name = "Teléfono")]
    [StringLength(15)]
    public string? Telefono { get; set; }

    [Display(Name = "Correo")]
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    [StringLength(100)]
    public string Correo { get; set; } = string.Empty;

    [Display(Name = "Contraseña")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres.")]
    public string? Password { get; set; }

    [Display(Name = "Repetir contraseña")]
    public string? ConfirmPassword { get; set; }

    [Display(Name = "Tiempo de atención (min)")]
    [Required(ErrorMessage = "El tiempo de atención es obligatorio.")]
    [Range(1, 60, ErrorMessage = "El tiempo de atención debe ser entre 1 y 60 minutos.")]
    public int TiempoAtencionMin { get; set; } = 60;

    public bool Estado { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Password))
        {
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                yield return new ValidationResult("Confirma la contraseña.", new[] { nameof(ConfirmPassword) });
            }
            else if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
            {
                yield return new ValidationResult("Las contraseñas no coinciden.", new[] { nameof(ConfirmPassword) });
            }
        }
    }
}
