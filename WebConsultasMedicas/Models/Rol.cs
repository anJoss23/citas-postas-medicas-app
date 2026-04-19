namespace WebConsultasMedicas.Models;

using System.ComponentModel.DataAnnotations;

public class Rol
{
    public int IdRol { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no debe exceder 50 caracteres.")]
    public string Nombre { get; set; } = null!;

    public bool Estado { get; set; } = true;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
