using System.ComponentModel.DataAnnotations;

namespace WebConsultasMedicas.Models;

public class PacienteCambiarClaveViewModel
{
    public int IdPaciente { get; set; }
    public string Paciente { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres.")]
    public string NuevaClave { get; set; } = string.Empty;

    [Compare(nameof(NuevaClave), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarClave { get; set; } = string.Empty;
}

