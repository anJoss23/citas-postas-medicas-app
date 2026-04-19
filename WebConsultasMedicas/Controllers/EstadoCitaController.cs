using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;

namespace WebConsultasMedicas.Controllers;

public class EstadoCitaController : Controller
{
    private readonly ApplicationDbContext _context;

    public EstadoCitaController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Estado de cita";
        var items = await _context.EstadosCita.AsNoTracking().OrderBy(e => e.IdEstadoCita).ToListAsync();
        return View(items);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Nuevo estado de cita";
        return View(new EstadoCita());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre")] EstadoCita estadoCita)
    {
        ViewData["Title"] = "Nuevo estado de cita";
        if (!ModelState.IsValid) return View(estadoCita);

        _context.EstadosCita.Add(estadoCita);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Estado de cita registrado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(EstadoCita.Nombre), "No se pudo guardar. Verifica que el nombre no esté duplicado.");
            return View(estadoCita);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var estado = await _context.EstadosCita.FindAsync(id.Value);
        if (estado is null) return NotFound();
        ViewData["Title"] = "Editar estado de cita";
        return View(estado);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdEstadoCita,Nombre")] EstadoCita estadoCita)
    {
        if (id != estadoCita.IdEstadoCita) return NotFound();
        ViewData["Title"] = "Editar estado de cita";
        if (!ModelState.IsValid) return View(estadoCita);

        _context.Entry(estadoCita).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Estado de cita actualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(EstadoCita.Nombre), "No se pudo guardar. Verifica que el nombre no esté duplicado.");
            return View(estadoCita);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var estado = await _context.EstadosCita.AsNoTracking().FirstOrDefaultAsync(e => e.IdEstadoCita == id.Value);
        if (estado is null) return NotFound();
        ViewData["Title"] = "Eliminar estado de cita";
        return View(estado);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var estado = await _context.EstadosCita.FindAsync(id);
        if (estado is null) return RedirectToAction(nameof(Index));

        _context.EstadosCita.Remove(estado);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Estado de cita eliminado.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo eliminar. Puede estar relacionado a citas.";
        }

        return RedirectToAction(nameof(Index));
    }
}

