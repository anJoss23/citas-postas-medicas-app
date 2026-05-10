using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebConsultasMedicas.Models;

public class MedicoIndicacionViewModel
{
    public int IdCita { get; set; }

    public CitaMedica? Cita { get; set; }

    public bool IsReadOnly { get; set; }

    [Required(ErrorMessage = "Selecciona un estado.")]
    public int IdEstadoCita { get; set; }

    [StringLength(250, ErrorMessage = "La observación no debe exceder 250 caracteres.")]
    public string? Observacion { get; set; }

    public DateTime? FechaAtencion { get; set; }

    public SelectList? Estados { get; set; }
}

