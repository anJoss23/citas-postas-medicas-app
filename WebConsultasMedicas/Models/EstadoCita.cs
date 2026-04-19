namespace WebConsultasMedicas.Models;

using System.ComponentModel.DataAnnotations;

public class EstadoCita
{
    public int IdEstadoCita { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no debe exceder 50 caracteres.")]
    public string Nombre { get; set; } = null!;

    public ICollection<CitaMedica> Citas { get; set; } = new List<CitaMedica>();
    public ICollection<HistorialCita> Historiales { get; set; } = new List<HistorialCita>();
}
