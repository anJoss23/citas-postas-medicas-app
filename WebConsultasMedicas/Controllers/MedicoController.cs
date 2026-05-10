using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;
using WebConsultasMedicas.Security;

namespace WebConsultasMedicas.Controllers;

[Authorize(Roles = "Administrador")]
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
            .Include(m => m.Usuario)
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
        return View(new MedicoAdminViewModel { Estado = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MedicoAdminViewModel model)
    {
        ViewData["Title"] = "Nuevo médico";
        await LoadEspecialidadesAsync(model.IdEspecialidad);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var medicoRoleId = await _context.Roles.Where(r => r.Nombre == "Medico").Select(r => r.IdRol).FirstOrDefaultAsync();
        if (medicoRoleId == 0)
        {
            TempData["Error"] = "No existe el rol Medico en la base de datos.";
            return View(model);
        }

        var email = model.Correo.Trim();
        if (!email.Contains('@'))
        {
            email = $"{email}@siscitasweb.local";
        }
        email = email.ToLowerInvariant();

        if (await _context.Usuarios.AnyAsync(u => u.Correo.ToLower() == email))
        {
            ModelState.AddModelError(nameof(MedicoAdminViewModel.Correo), "El correo ya está registrado.");
            return View(model);
        }

        var generatedPassword = string.IsNullOrWhiteSpace(model.Password) ? "medico123" : model.Password.Trim();

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var usuario = new Usuario
            {
                Correo = email,
                ClaveHash = PasswordHasher.Sha256Hex(generatedPassword),
                IdRol = medicoRoleId,
                Estado = true,
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var medico = new Medico
            {
                IdUsuario = usuario.IdUsuario,
                IdEspecialidad = model.IdEspecialidad,
                CMP = model.CMP.Trim(),
                Nombres = model.Nombres.Trim(),
                ApellidoPaterno = model.ApellidoPaterno.Trim(),
                ApellidoMaterno = model.ApellidoMaterno.Trim(),
                Telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim(),
                Correo = email,
                Estado = model.Estado
            };

            _context.Medicos.Add(medico);
            await _context.SaveChangesAsync();

            await tx.CommitAsync();

            TempData["Success"] = $"Médico registrado. Credenciales: {email} / {generatedPassword}";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            TempData["Error"] = "No se pudo guardar el médico. Verifica CMP/correo duplicado o restricciones.";
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var medico = await _context.Medicos
            .Include(m => m.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdMedico == id.Value);

        if (medico is null) return NotFound();

        ViewData["Title"] = "Editar médico";
        await LoadEspecialidadesAsync(medico.IdEspecialidad);

        return View(new MedicoAdminViewModel
        {
            IdMedico = medico.IdMedico,
            IdEspecialidad = medico.IdEspecialidad,
            CMP = medico.CMP,
            Nombres = medico.Nombres,
            ApellidoPaterno = medico.ApellidoPaterno,
            ApellidoMaterno = medico.ApellidoMaterno,
            Telefono = medico.Telefono,
            Correo = medico.Usuario?.Correo ?? medico.Correo ?? string.Empty,
            Estado = medico.Estado
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MedicoAdminViewModel model)
    {
        if (id != model.IdMedico) return NotFound();

        ViewData["Title"] = "Editar médico";
        await LoadEspecialidadesAsync(model.IdEspecialidad);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var medico = await _context.Medicos.Include(m => m.Usuario).FirstOrDefaultAsync(m => m.IdMedico == id);
        if (medico is null) return NotFound();

        var email = model.Correo.Trim();
        if (!email.Contains('@'))
        {
            email = $"{email}@siscitasweb.local";
        }
        email = email.ToLowerInvariant();

        var existsOtherUser = await _context.Usuarios.AnyAsync(u => u.Correo.ToLower() == email && u.IdUsuario != medico.IdUsuario);
        if (existsOtherUser)
        {
            ModelState.AddModelError(nameof(MedicoAdminViewModel.Correo), "El correo ya está registrado.");
            return View(model);
        }

        medico.IdEspecialidad = model.IdEspecialidad;
        medico.CMP = model.CMP.Trim();
        medico.Nombres = model.Nombres.Trim();
        medico.ApellidoPaterno = model.ApellidoPaterno.Trim();
        medico.ApellidoMaterno = model.ApellidoMaterno.Trim();
        medico.Telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim();
        medico.Correo = email;
        medico.Estado = model.Estado;

        if (medico.Usuario is not null)
        {
            medico.Usuario.Correo = email;
        }

        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Médico actualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo guardar el médico.";
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var item = await _context.Medicos
            .Include(m => m.Especialidad)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdMedico == id.Value);

        if (item is null) return NotFound();

        ViewData["Title"] = "Eliminar médico";
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Medicos.FindAsync(id);
        if (item is null) return RedirectToAction(nameof(Index));

        _context.Medicos.Remove(item);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Médico eliminado.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo eliminar. Puede estar relacionado a citas/horarios.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadEspecialidadesAsync(int? selected = null)
    {
        var especialidades = await _context.Especialidades.AsNoTracking()
            .Where(e => e.Estado)
            .OrderBy(e => e.Nombre)
            .ToListAsync();

        ViewBag.Especialidades = new SelectList(especialidades, nameof(Especialidad.IdEspecialidad), nameof(Especialidad.Nombre), selected);
    }
}

