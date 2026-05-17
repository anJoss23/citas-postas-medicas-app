using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebConsultasMedicas.Controllers;

[Authorize(Roles = "Administrador")]
public class HorarioMedicoController : Controller
{
    private readonly ApplicationDbContext _context;

    public HorarioMedicoController(ApplicationDbContext context)
    {
        _context = context;
}

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Horarios médicos";
        var items = await _context.HorariosMedicos
            .Include(h => h.Medico)
            .Include(h => h.Especialidad)
            .Include(h => h.Turno)
            .AsNoTracking()
            .OrderBy(h => h.IdMedico)
            .ThenBy(h => h.Fecha)
            .ThenBy(h => h.HoraInicio)
            .ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Nuevo horario";
        await LoadCombosAsync();
        return View(new HorarioMedico { Estado = true, Fecha = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdMedico,IdTurno,Fecha,HoraInicio,HoraFin,Estado")] HorarioMedico item)
    {
        ViewData["Title"] = "Nuevo horario";
        await LoadCombosAsync(item.IdMedico, item.IdTurno);

        var medicoInfo = await _context.Medicos.AsNoTracking()
            .Where(m => m.IdMedico == item.IdMedico)
            .Select(m => new { m.IdEspecialidad, m.TiempoAtencionMin })
            .FirstOrDefaultAsync();

        if (medicoInfo is null)
        {
            TempData["Error"] = "Médico inválido.";
            return View(item);
        }

        item.IdEspecialidad = medicoInfo.IdEspecialidad;
        var tiempoAtencionMin = medicoInfo.TiempoAtencionMin;
        if (tiempoAtencionMin <= 0 || tiempoAtencionMin > 60) tiempoAtencionMin = 60;

        ModelState.Remove(nameof(HorarioMedico.IdEspecialidad));
        ModelState.Remove(nameof(HorarioMedico.Medico));
        ModelState.Remove(nameof(HorarioMedico.Especialidad));
        ModelState.Remove(nameof(HorarioMedico.Turno));
        if (!ModelState.IsValid)
        {
            var details = string.Join(" | ",
                ModelState
                    .Where(kv => kv.Value?.Errors.Count > 0)
                    .SelectMany(kv => kv.Value!.Errors.Select(e => $"{kv.Key}: {e.ErrorMessage}"))
            );
            TempData["Error"] = string.IsNullOrWhiteSpace(details) ? "Formulario inválido." : details;
            return View(item);
        }

        if (item.HoraFin <= item.HoraInicio)
        {
            ModelState.AddModelError(nameof(HorarioMedico.HoraFin), "La hora fin debe ser mayor a la hora inicio.");
            return View(item);
        }

        var totalMinutes = (item.HoraFin.ToTimeSpan() - item.HoraInicio.ToTimeSpan()).TotalMinutes;
        if (totalMinutes % tiempoAtencionMin != 0)
        {
            ModelState.AddModelError(nameof(HorarioMedico.HoraFin), $"El rango debe ser múltiplo de {tiempoAtencionMin} minuto(s).");
            return View(item);
        }

        var slots = new List<HorarioMedico>();
        var start = item.HoraInicio;
        while (start < item.HoraFin)
        {
            var end = start.AddMinutes(tiempoAtencionMin);
            if (end > item.HoraFin) break;

            slots.Add(new HorarioMedico
            {
                IdMedico = item.IdMedico,
                IdEspecialidad = item.IdEspecialidad,
                IdTurno = item.IdTurno,
                Fecha = item.Fecha,
                HoraInicio = start,
                HoraFin = end,
                Estado = item.Estado
            });

            start = end;
        }

        // Consulta simple (sin IN/OPENJSON): trae horas del día y filtra en memoria.
        var existingAllStarts = await _context.HorariosMedicos.AsNoTracking()
            .Where(h => h.IdMedico == item.IdMedico && h.Fecha == item.Fecha)
            .Select(h => h.HoraInicio)
            .ToListAsync();

        var slotStarts = slots.Select(s => s.HoraInicio).ToHashSet();
        var existingStarts = existingAllStarts
            .Where(t => slotStarts.Contains(t))
            .ToList();

        if (existingStarts.Count > 0)
        {
            TempData["Error"] = $"Ya existen horarios para esas horas: {string.Join(", ", existingStarts.Select(t => t.ToString("HH:mm")))}.";
            return View(item);
        }

        if (slots.Count == 0)
        {
            TempData["Error"] = $"No se generaron bloques. Verifica que el rango sea múltiplo de {tiempoAtencionMin} minuto(s).";
            return View(item);
        }

        if (item.IdEspecialidad <= 0)
        {
            TempData["Error"] = "No se pudo obtener la especialidad del médico seleccionado.";
            return View(item);
        }

        _context.HorariosMedicos.AddRange(slots);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Horarios registrados: {slots.Count} bloque(s) de {tiempoAtencionMin} minuto(s).";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo guardar. Verifica las restricciones del horario.";
            return View(item);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var item = await _context.HorariosMedicos.FindAsync(id.Value);
        if (item is null) return NotFound();

        ViewData["Title"] = "Editar horario";
        await LoadCombosAsync(item.IdMedico, item.IdTurno);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdHorarioMedico,IdMedico,IdTurno,Fecha,HoraInicio,HoraFin,Estado")] HorarioMedico item)
    {
        if (id != item.IdHorarioMedico) return NotFound();

        ViewData["Title"] = "Editar horario";
        await LoadCombosAsync(item.IdMedico, item.IdTurno);

        var medicoInfo = await _context.Medicos.AsNoTracking()
            .Where(m => m.IdMedico == item.IdMedico)
            .Select(m => new { m.IdEspecialidad })
            .FirstOrDefaultAsync();

        if (medicoInfo is null)
        {
            TempData["Error"] = "Médico inválido.";
            return View(item);
        }

        item.IdEspecialidad = medicoInfo.IdEspecialidad;

        ModelState.Remove(nameof(HorarioMedico.IdEspecialidad));
        ModelState.Remove(nameof(HorarioMedico.Medico));
        ModelState.Remove(nameof(HorarioMedico.Especialidad));
        ModelState.Remove(nameof(HorarioMedico.Turno));
        if (!ModelState.IsValid)
        {
            var details = string.Join(" | ",
                ModelState
                    .Where(kv => kv.Value?.Errors.Count > 0)
                    .SelectMany(kv => kv.Value!.Errors.Select(e => $"{kv.Key}: {e.ErrorMessage}"))
            );
            TempData["Error"] = string.IsNullOrWhiteSpace(details) ? "Formulario inválido." : details;
            return View(item);
        }

        if (item.HoraFin <= item.HoraInicio)
        {
            ModelState.AddModelError(nameof(HorarioMedico.HoraFin), "La hora fin debe ser mayor a la hora inicio.");
            return View(item);
        }

        _context.Entry(item).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Horario actualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo guardar. Verifica las restricciones del horario.";
            return View(item);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var item = await _context.HorariosMedicos
            .Include(h => h.Medico)
            .Include(h => h.Especialidad)
            .Include(h => h.Turno)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.IdHorarioMedico == id.Value);
        if (item is null) return NotFound();

        ViewData["Title"] = "Eliminar horario";
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.HorariosMedicos.FindAsync(id);
        if (item is null) return RedirectToAction(nameof(Index));

        _context.HorariosMedicos.Remove(item);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Horario eliminado.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo eliminar. Puede estar relacionado a citas.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCombosAsync(int? selectedMedico = null, int? selectedTurno = null)
    {
        var medicos = await _context.Medicos.AsNoTracking()
            .Include(m => m.Especialidad)
            .OrderBy(m => m.ApellidoPaterno)
            .ThenBy(m => m.Nombres)
            .Select(m => new
            {
                m.IdMedico,
                Nombre = $"{m.ApellidoPaterno} {m.ApellidoMaterno}, {m.Nombres} ({m.Especialidad.Nombre})"
            })
            .ToListAsync();

        var turnos = await _context.Turnos.AsNoTracking()
            .OrderBy(t => t.IdTurno)
            .Select(t => new
            {
                t.IdTurno,
                Nombre = $"{t.Nombre} ({t.HoraInicio:hh\\:mm}-{t.HoraFin:hh\\:mm})"
            })
            .ToListAsync();

        ViewBag.Medicos = new SelectList(medicos, "IdMedico", "Nombre", selectedMedico);
        ViewBag.Turnos = new SelectList(turnos, "IdTurno", "Nombre", selectedTurno);
    }
}
