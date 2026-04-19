using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;

namespace WebConsultasMedicas.Controllers;

public class RolController : Controller
{
    private readonly ApplicationDbContext _context;

    public RolController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Roles";
        var items = await _context.Roles.AsNoTracking().OrderBy(r => r.Nombre).ToListAsync();
        return View(items);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Nuevo rol";
        return View(new Rol { Estado = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Estado")] Rol rol)
    {
        ViewData["Title"] = "Nuevo rol";
        if (!ModelState.IsValid)
        {
            return View(rol);
        }

        _context.Roles.Add(rol);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Rol registrado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Rol.Nombre), "No se pudo guardar. Verifica que el nombre no esté duplicado.");
            return View(rol);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var rol = await _context.Roles.FindAsync(id.Value);
        if (rol is null) return NotFound();
        ViewData["Title"] = "Editar rol";
        return View(rol);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdRol,Nombre,Estado")] Rol rol)
    {
        if (id != rol.IdRol) return NotFound();
        ViewData["Title"] = "Editar rol";
        if (!ModelState.IsValid) return View(rol);

        _context.Entry(rol).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Rol actualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Rol.Nombre), "No se pudo guardar. Verifica que el nombre no esté duplicado.");
            return View(rol);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var rol = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.IdRol == id.Value);
        if (rol is null) return NotFound();
        ViewData["Title"] = "Eliminar rol";
        return View(rol);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var rol = await _context.Roles.FindAsync(id);
        if (rol is null) return RedirectToAction(nameof(Index));

        _context.Roles.Remove(rol);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Rol eliminado.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo eliminar. Puede estar relacionado a usuarios.";
        }

        return RedirectToAction(nameof(Index));
    }
}

