using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;
using WebConsultasMedicas.Security;

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleEstado(int idPaciente)
    {
        var paciente = await _context.Pacientes
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.IdPaciente == idPaciente);

        if (paciente is null) return NotFound();

        paciente.Estado = !paciente.Estado;
        if (paciente.Usuario is not null)
        {
            paciente.Usuario.Estado = paciente.Estado;
        }

        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = paciente.Estado ? "Paciente activado." : "Paciente inactivado.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo actualizar el estado del paciente.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CambiarClave(int idPaciente)
    {
        var item = await _context.Pacientes.AsNoTracking()
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.IdPaciente == idPaciente);

        if (item is null) return NotFound();

        ViewData["Title"] = "Cambiar contraseña";
        return View(new PacienteCambiarClaveViewModel
        {
            IdPaciente = item.IdPaciente,
            Paciente = $"{item.ApellidoPaterno} {item.ApellidoMaterno}, {item.Nombres}",
            Correo = item.Usuario?.Correo ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarClave(PacienteCambiarClaveViewModel model)
    {
        ViewData["Title"] = "Cambiar contraseña";
        if (!ModelState.IsValid) return View(model);

        var paciente = await _context.Pacientes
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.IdPaciente == model.IdPaciente);

        if (paciente?.Usuario is null) return NotFound();

        paciente.Usuario.ClaveHash = PasswordHasher.Sha256Hex(model.NuevaClave);

        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Contraseña actualizada.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo actualizar la contraseña.";
            return View(model);
        }
    }
}

