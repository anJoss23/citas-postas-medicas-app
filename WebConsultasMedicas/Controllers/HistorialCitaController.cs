using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebConsultasMedicas.Controllers;

[Authorize(Roles = "Administrador")]
public class HistorialCitaController : Controller
{
    private readonly ApplicationDbContext _context;

    public HistorialCitaController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Historial de citas";
        var items = await _context.HistorialCitas
            .Include(h => h.CitaMedica)
            .ThenInclude(c => c.Paciente)
            .Include(h => h.EstadoCita)
            .AsNoTracking()
            .OrderByDescending(h => h.FechaCambio)
            .Take(300)
            .ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Nuevo historial";
        await LoadCombosAsync();
        return View(new HistorialCita { FechaCambio = DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdCita,IdEstadoCita,FechaCambio,Observacion,UsuarioAccion")] HistorialCita item)
    {
        ViewData["Title"] = "Nuevo historial";
        await LoadCombosAsync(item.IdCita, item.IdEstadoCita);
        if (!ModelState.IsValid) return View(item);

        _context.HistorialCitas.Add(item);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Historial registrado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo guardar el historial.";
            return View(item);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var item = await _context.HistorialCitas.FindAsync(id.Value);
        if (item is null) return NotFound();

        ViewData["Title"] = "Editar historial";
        await LoadCombosAsync(item.IdCita, item.IdEstadoCita);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdHistorial,IdCita,IdEstadoCita,FechaCambio,Observacion,UsuarioAccion")] HistorialCita item)
    {
        if (id != item.IdHistorial) return NotFound();

        ViewData["Title"] = "Editar historial";
        await LoadCombosAsync(item.IdCita, item.IdEstadoCita);
        if (!ModelState.IsValid) return View(item);

        _context.Entry(item).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Historial actualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo guardar el historial.";
            return View(item);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var item = await _context.HistorialCitas
            .Include(h => h.CitaMedica)
            .ThenInclude(c => c.Paciente)
            .Include(h => h.EstadoCita)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.IdHistorial == id.Value);
        if (item is null) return NotFound();

        ViewData["Title"] = "Eliminar historial";
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.HistorialCitas.FindAsync(id);
        if (item is null) return RedirectToAction(nameof(Index));

        _context.HistorialCitas.Remove(item);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Historial eliminado.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo eliminar el historial.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCombosAsync(int? selectedCita = null, int? selectedEstado = null)
    {
        var citas = await _context.CitasMedicas.AsNoTracking()
            .Include(c => c.Paciente)
            .OrderByDescending(c => c.FechaCita)
            .ThenByDescending(c => c.HoraCita)
            .Take(300)
            .Select(c => new
            {
                c.IdCita,
                Nombre = $"{c.IdCita} - {c.Paciente.ApellidoPaterno} {c.Paciente.ApellidoMaterno}, {c.Paciente.Nombres} ({c.FechaCita:yyyy-MM-dd} {c.HoraCita:hh\\:mm})"
            })
            .ToListAsync();

        var estados = await _context.EstadosCita.AsNoTracking()
            .OrderBy(e => e.IdEstadoCita)
            .ToListAsync();

        ViewBag.Citas = new SelectList(citas, "IdCita", "Nombre", selectedCita);
        ViewBag.Estados = new SelectList(estados, nameof(EstadoCita.IdEstadoCita), nameof(EstadoCita.Nombre), selectedEstado);
    }
}
