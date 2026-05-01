using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebConsultasMedicas.Controllers;

[Authorize(Roles = "Administrador")]
public class CitaMedicaController : Controller
{
    private readonly ApplicationDbContext _context;

    public CitaMedicaController(ApplicationDbContext context)
    {
        _context = context;
}

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Citas médicas";
        var items = await _context.CitasMedicas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .Include(c => c.Especialidad)
            .Include(c => c.EstadoCita)
            .AsNoTracking()
            .OrderByDescending(c => c.FechaCita)
            .ThenByDescending(c => c.HoraCita)
            .Take(200)
            .ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Nueva cita";
        await LoadCombosAsync();
        return View(new CitaMedica
        {
            FechaCita = DateOnly.FromDateTime(DateTime.Today),
            HoraCita = new TimeOnly(8, 0),
            IdEstadoCita = 1
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdPaciente,IdHorarioMedico,IdEstadoCita,FechaCita,HoraCita,MotivoConsulta,Observacion")] CitaMedica item)
    {
        ViewData["Title"] = "Nueva cita";
        await LoadCombosAsync(item.IdPaciente, item.IdHorarioMedico, item.IdEstadoCita);

        var horarioInfo = await _context.HorariosMedicos.AsNoTracking()
            .Where(h => h.IdHorarioMedico == item.IdHorarioMedico)
            .Select(h => new { h.IdMedico, h.IdEspecialidad })
            .FirstOrDefaultAsync();

        if (horarioInfo is null)
        {
            ModelState.AddModelError(nameof(CitaMedica.IdHorarioMedico), "Horario médico inválido.");
            return View(item);
        }

        item.IdMedico = horarioInfo.IdMedico;
        item.IdEspecialidad = horarioInfo.IdEspecialidad;
        ModelState.Remove(nameof(CitaMedica.IdMedico));
        ModelState.Remove(nameof(CitaMedica.IdEspecialidad));
        if (!ModelState.IsValid) return View(item);

        _context.CitasMedicas.Add(item);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cita registrada.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo guardar. Verifica duplicados/horarios.";
            return View(item);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var item = await _context.CitasMedicas.FindAsync(id.Value);
        if (item is null) return NotFound();

        ViewData["Title"] = "Editar cita";
        await LoadCombosAsync(item.IdPaciente, item.IdHorarioMedico, item.IdEstadoCita);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdCita,IdPaciente,IdHorarioMedico,IdEstadoCita,FechaCita,HoraCita,MotivoConsulta,Observacion,FechaRegistro")] CitaMedica item)
    {
        if (id != item.IdCita) return NotFound();

        ViewData["Title"] = "Editar cita";
        await LoadCombosAsync(item.IdPaciente, item.IdHorarioMedico, item.IdEstadoCita);

        var horarioInfo = await _context.HorariosMedicos.AsNoTracking()
            .Where(h => h.IdHorarioMedico == item.IdHorarioMedico)
            .Select(h => new { h.IdMedico, h.IdEspecialidad })
            .FirstOrDefaultAsync();

        if (horarioInfo is null)
        {
            ModelState.AddModelError(nameof(CitaMedica.IdHorarioMedico), "Horario médico inválido.");
            return View(item);
        }

        item.IdMedico = horarioInfo.IdMedico;
        item.IdEspecialidad = horarioInfo.IdEspecialidad;
        ModelState.Remove(nameof(CitaMedica.IdMedico));
        ModelState.Remove(nameof(CitaMedica.IdEspecialidad));
        if (!ModelState.IsValid) return View(item);

        _context.Entry(item).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cita actualizada.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo guardar. Verifica duplicados/horarios.";
            return View(item);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var item = await _context.CitasMedicas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .Include(c => c.Especialidad)
            .Include(c => c.EstadoCita)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCita == id.Value);
        if (item is null) return NotFound();

        ViewData["Title"] = "Eliminar cita";
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.CitasMedicas.FindAsync(id);
        if (item is null) return RedirectToAction(nameof(Index));

        _context.CitasMedicas.Remove(item);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cita eliminada.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo eliminar. Puede estar relacionada a historial.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCombosAsync(
        int? selectedPaciente = null,
        int? selectedHorario = null,
        int? selectedEstado = null)
    {
        var pacientes = await _context.Pacientes.AsNoTracking()
            .OrderBy(p => p.ApellidoPaterno)
            .ThenBy(p => p.Nombres)
            .Select(p => new
            {
                p.IdPaciente,
                Nombre = $"{p.ApellidoPaterno} {p.ApellidoMaterno}, {p.Nombres} ({p.DNI})"
            })
            .ToListAsync();

        var horarios = await _context.HorariosMedicos.AsNoTracking()
            .Include(h => h.Medico)
            .Include(h => h.Especialidad)
            .OrderBy(h => h.IdHorarioMedico)
            .Select(h => new
            {
                h.IdHorarioMedico,
                Nombre = $"{h.IdHorarioMedico} - {h.Medico.ApellidoPaterno} {h.Medico.ApellidoMaterno}, {h.Medico.Nombres} / {h.Especialidad.Nombre} ({h.Fecha:yyyy-MM-dd} {h.HoraInicio:hh\\:mm}-{h.HoraFin:hh\\:mm})"
            })
            .ToListAsync();

        var estados = await _context.EstadosCita.AsNoTracking()
            .OrderBy(e => e.IdEstadoCita)
            .ToListAsync();

        ViewBag.Pacientes = new SelectList(pacientes, "IdPaciente", "Nombre", selectedPaciente);
        ViewBag.Horarios = new SelectList(horarios, "IdHorarioMedico", "Nombre", selectedHorario);
        ViewBag.Estados = new SelectList(estados, nameof(EstadoCita.IdEstadoCita), nameof(EstadoCita.Nombre), selectedEstado);
    }
}
