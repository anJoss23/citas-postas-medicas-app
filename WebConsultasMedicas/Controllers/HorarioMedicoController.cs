using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;

namespace WebConsultasMedicas.Controllers;

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
            .ThenBy(h => h.DiaSemana)
            .ThenBy(h => h.HoraInicio)
            .ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Nuevo horario";
        await LoadCombosAsync();
        return View(new HorarioMedico { Estado = true, DiaSemana = 1, Cupos = 10 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdMedico,IdTurno,DiaSemana,HoraInicio,HoraFin,Cupos,Estado")] HorarioMedico item)
    {
        ViewData["Title"] = "Nuevo horario";
        await LoadCombosAsync(item.IdMedico, item.IdTurno);

        item.IdEspecialidad = await _context.Medicos
            .Where(m => m.IdMedico == item.IdMedico)
            .Select(m => m.IdEspecialidad)
            .FirstOrDefaultAsync();

        ModelState.Remove(nameof(HorarioMedico.IdEspecialidad));
        if (!ModelState.IsValid) return View(item);

        _context.HorariosMedicos.Add(item);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Horario registrado.";
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
    public async Task<IActionResult> Edit(int id, [Bind("IdHorarioMedico,IdMedico,IdTurno,DiaSemana,HoraInicio,HoraFin,Cupos,Estado")] HorarioMedico item)
    {
        if (id != item.IdHorarioMedico) return NotFound();

        ViewData["Title"] = "Editar horario";
        await LoadCombosAsync(item.IdMedico, item.IdTurno);

        item.IdEspecialidad = await _context.Medicos
            .Where(m => m.IdMedico == item.IdMedico)
            .Select(m => m.IdEspecialidad)
            .FirstOrDefaultAsync();

        ModelState.Remove(nameof(HorarioMedico.IdEspecialidad));
        if (!ModelState.IsValid) return View(item);

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
