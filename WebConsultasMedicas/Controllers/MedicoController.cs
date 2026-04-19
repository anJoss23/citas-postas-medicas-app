using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;

namespace WebConsultasMedicas.Controllers;

public class MedicoController : Controller
{
    private readonly ApplicationDbContext _context;

    public MedicoController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Médicos";
        var items = await _context.Medicos
            .Include(m => m.Especialidad)
            .AsNoTracking()
            .OrderBy(m => m.ApellidoPaterno)
            .ThenBy(m => m.Nombres)
            .ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Nuevo médico";
        await LoadEspecialidadesAsync();
        return View(new Medico { Estado = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdEspecialidad,CMP,Nombres,ApellidoPaterno,ApellidoMaterno,Telefono,Correo,Estado")] Medico medico)
    {
        ViewData["Title"] = "Nuevo médico";
        await LoadEspecialidadesAsync(medico.IdEspecialidad);
        if (!ModelState.IsValid) return View(medico);

        _context.Medicos.Add(medico);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Médico registrado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Medico.CMP), "No se pudo guardar. Verifica que el CMP no esté duplicado.");
            return View(medico);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var medico = await _context.Medicos.FindAsync(id.Value);
        if (medico is null) return NotFound();
        ViewData["Title"] = "Editar médico";
        await LoadEspecialidadesAsync(medico.IdEspecialidad);
        return View(medico);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdMedico,IdEspecialidad,CMP,Nombres,ApellidoPaterno,ApellidoMaterno,Telefono,Correo,Estado")] Medico medico)
    {
        if (id != medico.IdMedico) return NotFound();
        ViewData["Title"] = "Editar médico";
        await LoadEspecialidadesAsync(medico.IdEspecialidad);
        if (!ModelState.IsValid) return View(medico);

        _context.Entry(medico).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Médico actualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Medico.CMP), "No se pudo guardar. Verifica que el CMP no esté duplicado.");
            return View(medico);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var medico = await _context.Medicos
            .Include(m => m.Especialidad)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdMedico == id.Value);
        if (medico is null) return NotFound();
        ViewData["Title"] = "Eliminar médico";
        return View(medico);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var medico = await _context.Medicos.FindAsync(id);
        if (medico is null) return RedirectToAction(nameof(Index));

        _context.Medicos.Remove(medico);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Médico eliminado.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo eliminar. Puede estar relacionado a horarios o citas.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadEspecialidadesAsync(int? selectedId = null)
    {
        var items = await _context.Especialidades.AsNoTracking()
            .Where(e => e.Estado)
            .OrderBy(e => e.Nombre)
            .ToListAsync();
        ViewBag.Especialidades = new SelectList(items, nameof(Especialidad.IdEspecialidad), nameof(Especialidad.Nombre), selectedId);
    }
}
