using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebConsultasMedicas.Models.Citas;

public class CitaNuevaViewModel
{
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateOnly FechaCita { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public int? IdPaciente { get; set; }

    [Required(ErrorMessage = "La especialidad es obligatoria.")]
    public int? IdEspecialidad { get; set; }

    [Required(ErrorMessage = "El médico es obligatorio.")]
    public int? IdMedico { get; set; }

    [Required(ErrorMessage = "El horario es obligatorio.")]
    public int? IdHorarioMedico { get; set; }

    public SelectList Pacientes { get; set; } = new(Array.Empty<SelectListItem>());
    public SelectList Especialidades { get; set; } = new(Array.Empty<SelectListItem>());
    public SelectList Medicos { get; set; } = new(Array.Empty<SelectListItem>());
    public SelectList Horarios { get; set; } = new(Array.Empty<SelectListItem>());

    public bool CanReserve => IdPaciente.GetValueOrDefault() > 0 && IdEspecialidad.GetValueOrDefault() > 0 && IdMedico.GetValueOrDefault() > 0;
}

