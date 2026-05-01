using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebConsultasMedicas.Controllers;

[Authorize(Roles = "Administrador")]
public class PacienteController : Controller
{
    private readonly ApplicationDbContext _context;

    public PacienteController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Pacientes";
        var items = await _context.Pacientes
            .Include(p => p.Usuario)
            .AsNoTracking()
            .OrderBy(p => p.ApellidoPaterno)
            .ThenBy(p => p.Nombres)
            .ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Nuevo paciente";
        await LoadUsuariosAsync();
        return View(new Paciente { Estado = true, FechaNacimiento = DateOnly.FromDateTime(DateTime.Today) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdUsuario,DNI,Nombres,ApellidoPaterno,ApellidoMaterno,FechaNacimiento,Sexo,Telefono,Direccion,NumeroSIS,Estado")] Paciente paciente)
    {
        ViewData["Title"] = "Nuevo paciente";
        await LoadUsuariosAsync(paciente.IdUsuario);
        if (!ModelState.IsValid) return View(paciente);

        _context.Pacientes.Add(paciente);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Paciente registrado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo guardar. Verifica que el usuario, DNI o SIS no estén duplicados.";
            return View(paciente);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var paciente = await _context.Pacientes.FindAsync(id.Value);
        if (paciente is null) return NotFound();

        ViewData["Title"] = "Editar paciente";
        await LoadUsuariosAsync(paciente.IdUsuario, includeAssigned: true);
        return View(paciente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdPaciente,IdUsuario,DNI,Nombres,ApellidoPaterno,ApellidoMaterno,FechaNacimiento,Sexo,Telefono,Direccion,NumeroSIS,Estado")] Paciente paciente)
    {
        if (id != paciente.IdPaciente) return NotFound();

        ViewData["Title"] = "Editar paciente";
        await LoadUsuariosAsync(paciente.IdUsuario, includeAssigned: true);
        if (!ModelState.IsValid) return View(paciente);

        _context.Entry(paciente).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Paciente actualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo guardar. Verifica que el usuario, DNI o SIS no estén duplicados.";
            return View(paciente);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var paciente = await _context.Pacientes
            .Include(p => p.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPaciente == id.Value);
        if (paciente is null) return NotFound();

        ViewData["Title"] = "Eliminar paciente";
        return View(paciente);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);
        if (paciente is null) return RedirectToAction(nameof(Index));

        _context.Pacientes.Remove(paciente);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Paciente eliminado.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo eliminar. Puede estar relacionado a citas.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadUsuariosAsync(int? selectedId = null, bool includeAssigned = false)
    {
        IQueryable<Usuario> query = _context.Usuarios.AsNoTracking().OrderBy(u => u.Correo);

        if (!includeAssigned)
        {
            var assignedUserIds = _context.Pacientes.Select(p => p.IdUsuario);
            query = query.Where(u => !assignedUserIds.Contains(u.IdUsuario));
        }

        var usuarios = await query.ToListAsync();
        ViewBag.Usuarios = new SelectList(usuarios, nameof(Usuario.IdUsuario), nameof(Usuario.Correo), selectedId);
    }
}
