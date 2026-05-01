using System.ComponentModel.DataAnnotations;

namespace WebConsultasMedicas.Models;

public class MedicoAdminViewModel
{
    public int IdMedico { get; set; }

    [Required]
    public int IdEspecialidad { get; set; }

    [Required]
    [StringLength(20)]
    public string CMP { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ApellidoMaterno { get; set; } = string.Empty;

    [StringLength(15)]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    [StringLength(100)]
    public string Correo { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres.")]
    public string? Password { get; set; }

    public bool Estado { get; set; } = true;
}

