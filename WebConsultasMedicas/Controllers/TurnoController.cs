using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;

namespace WebConsultasMedicas.Controllers;

public class TurnoController : Controller
{
    private readonly ApplicationDbContext _context;

    public TurnoController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Turno";
        var items = await _context.Turnos.AsNoTracking().OrderBy(t => t.IdTurno).ToListAsync();
        return View(items);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Nuevo turno";
        return View(new Turno { Estado = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,HoraInicio,HoraFin,Estado")] Turno turno)
    {
        ViewData["Title"] = "Nuevo turno";
        if (!ModelState.IsValid) return View(turno);

        _context.Turnos.Add(turno);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Turno registrado.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var turno = await _context.Turnos.FindAsync(id.Value);
        if (turno is null) return NotFound();
        ViewData["Title"] = "Editar turno";
        return View(turno);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdTurno,Nombre,HoraInicio,HoraFin,Estado")] Turno turno)
    {
        if (id != turno.IdTurno) return NotFound();
        ViewData["Title"] = "Editar turno";
        if (!ModelState.IsValid) return View(turno);

        _context.Entry(turno).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Turno actualizado.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var turno = await _context.Turnos.AsNoTracking().FirstOrDefaultAsync(t => t.IdTurno == id.Value);
        if (turno is null) return NotFound();
        ViewData["Title"] = "Eliminar turno";
        return View(turno);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var turno = await _context.Turnos.FindAsync(id);
        if (turno is null) return RedirectToAction(nameof(Index));

        _context.Turnos.Remove(turno);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Turno eliminado.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo eliminar. Puede estar relacionado a horarios.";
        }

        return RedirectToAction(nameof(Index));
    }
}

