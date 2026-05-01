using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebConsultasMedicas.Controllers;

[Authorize(Roles = "Administrador")]
public class EspecialidadController : Controller
{
    private readonly ApplicationDbContext _context;

    public EspecialidadController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Especialidad";
        var items = await _context.Especialidades
            .OrderBy(e => e.Nombre)
            .AsNoTracking()
            .ToListAsync();
        return View(items);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Nueva especialidad";
        return View(new Especialidad { Estado = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Estado")] Especialidad especialidad)
    {
        ViewData["Title"] = "Nueva especialidad";

        if (!ModelState.IsValid)
        {
            return View(especialidad);
        }

        _context.Especialidades.Add(especialidad);

        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Especialidad registrada.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Especialidad.Nombre), "No se pudo guardar. Verifica que el nombre no esté duplicado.");
            return View(especialidad);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var especialidad = await _context.Especialidades.FindAsync(id.Value);
        if (especialidad is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Editar especialidad";
        return View(especialidad);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdEspecialidad,Nombre,Descripcion,Estado")] Especialidad especialidad)
    {
        if (id != especialidad.IdEspecialidad)
        {
            return NotFound();
        }

        ViewData["Title"] = "Editar especialidad";

        if (!ModelState.IsValid)
        {
            return View(especialidad);
        }

        _context.Entry(especialidad).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Especialidad actualizada.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Especialidad.Nombre), "No se pudo guardar. Verifica que el nombre no esté duplicado o que no haya restricciones.");
            return View(especialidad);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var especialidad = await _context.Especialidades
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdEspecialidad == id.Value);

        if (especialidad is null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Eliminar especialidad";
        return View(especialidad);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var especialidad = await _context.Especialidades.FindAsync(id);
        if (especialidad is null)
        {
            return RedirectToAction(nameof(Index));
        }

        _context.Especialidades.Remove(especialidad);

        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Especialidad eliminada.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo eliminar. Puede estar relacionada a citas u horarios.";
            return RedirectToAction(nameof(Index));
        }
    }
}
