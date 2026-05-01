using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebConsultasMedicas.Models;

public class PortalReservarViewModel
{
    [Required]
    public int IdHorarioMedico { get; set; }

    [Required]
    public DateOnly FechaCita { get; set; }

    [Required]
    public TimeOnly HoraCita { get; set; }

    [StringLength(250)]
    public string? MotivoConsulta { get; set; }

    [StringLength(250)]
    public string? Observacion { get; set; }

    public HorarioMedico? Horario { get; set; }
    public SelectList? HorasDisponibles { get; set; }
}

