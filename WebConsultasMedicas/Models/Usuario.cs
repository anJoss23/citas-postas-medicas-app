namespace WebConsultasMedicas.Models;

using System.ComponentModel.DataAnnotations;

public class Usuario
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [StringLength(100, ErrorMessage = "El correo no debe exceder 100 caracteres.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    public string Correo { get; set; } = null!;

    [Required(ErrorMessage = "La clave es obligatoria.")]
    [StringLength(255, ErrorMessage = "La clave no debe exceder 255 caracteres.")]
    public string ClaveHash { get; set; } = null!;

    [Required]
    public int IdRol { get; set; }

    public bool Estado { get; set; } = true;
    public DateTime FechaRegistro { get; set; }

    public Rol Rol { get; set; } = null!;
    public Paciente? Paciente { get; set; }
    public Medico? Medico { get; set; }
}
