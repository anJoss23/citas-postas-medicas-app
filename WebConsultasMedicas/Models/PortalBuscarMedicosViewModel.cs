using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebConsultasMedicas.Models;

public class PortalBuscarMedicosViewModel
{
    public int? IdEspecialidad { get; set; }
    public string? MedicoQuery { get; set; }

    public List<Medico> Medicos { get; set; } = new();
    public SelectList? Especialidades { get; set; }
}

