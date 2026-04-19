namespace WebConsultasMedicas.Models;

using System.ComponentModel.DataAnnotations;

public class HistorialCita
{
    public int IdHistorial { get; set; }

    [Required]
    public int IdCita { get; set; }

    [Required]
    public int IdEstadoCita { get; set; }
    public DateTime FechaCambio { get; set; }

    [StringLength(250, ErrorMessage = "La observación no debe exceder 250 caracteres.")]
    public string? Observacion { get; set; }

    [StringLength(100, ErrorMessage = "El usuario acción no debe exceder 100 caracteres.")]
    public string? UsuarioAccion { get; set; }

    public CitaMedica CitaMedica { get; set; } = null!;
    public EstadoCita EstadoCita { get; set; } = null!;
}
