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

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Nuevo paciente";
        return View(new AdminPacienteCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminPacienteCreateViewModel model)
    {
        ViewData["Title"] = "Nuevo paciente";
        if (!ModelState.IsValid) return View(model);

        var email = model.Correo.Trim();
        if (!email.Contains('@'))
        {
            email = $"{email}@siscitasweb.local";
        }
        email = email.ToLowerInvariant();
        if (await _context.Usuarios.AnyAsync(u => u.Correo.ToLower() == email))
        {
            ModelState.AddModelError(nameof(AdminPacienteCreateViewModel.Correo), "El correo ya está registrado.");
            return View(model);
        }

        if (await _context.Pacientes.AnyAsync(p => p.DNI == model.DNI))
        {
            ModelState.AddModelError(nameof(AdminPacienteCreateViewModel.DNI), "El DNI ya está registrado.");
            return View(model);
        }

        var pacienteRolId = await _context.Roles
            .Where(r => r.Nombre == "Paciente")
            .Select(r => r.IdRol)
            .FirstOrDefaultAsync();

        if (pacienteRolId == 0)
        {
            TempData["Error"] = "No existe el rol Paciente en la base de datos.";
            return View(model);
        }

        var usuario = new Usuario
        {
            Correo = email,
            ClaveHash = PasswordHasher.Sha256Hex(model.Password),
            IdRol = pacienteRolId,
            Estado = model.Estado,
            FechaRegistro = DateTime.Now
        };

        var paciente = new Paciente
        {
            Usuario = usuario,
            DNI = model.DNI,
            Nombres = model.Nombres,
            ApellidoPaterno = model.ApellidoPaterno,
            ApellidoMaterno = model.ApellidoMaterno,
            FechaNacimiento = model.FechaNacimiento,
            Sexo = model.Sexo,
            Telefono = model.Telefono,
            Direccion = model.Direccion,
            NumeroSIS = string.Empty,
            Estado = model.Estado
        };

        _context.Pacientes.Add(paciente);

        try
        {
            for (var attempt = 0; attempt < 3; attempt++)
            {
                paciente.NumeroSIS = await NumeroHistoriaClinicaGenerator.NextAsync(_context);
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Paciente registrado. HCL: {paciente.NumeroSIS}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    if (attempt == 2) throw;
                }
            }

            TempData["Success"] = "Paciente registrado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo registrar. Verifica duplicados.";
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var paciente = await _context.Pacientes.AsNoTracking()
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.IdPaciente == id);

        if (paciente is null) return NotFound();

        ViewData["Title"] = "Editar paciente";
        return View(new AdminPacienteEditViewModel
        {
            IdPaciente = paciente.IdPaciente,
            Correo = paciente.Usuario?.Correo ?? string.Empty,
            DNI = paciente.DNI,
            Nombres = paciente.Nombres,
            ApellidoPaterno = paciente.ApellidoPaterno,
            ApellidoMaterno = paciente.ApellidoMaterno,
            FechaNacimiento = paciente.FechaNacimiento,
            Sexo = paciente.Sexo,
            Telefono = paciente.Telefono,
            Direccion = paciente.Direccion,
            Estado = paciente.Estado,
            NumeroHistoriaClinica = paciente.NumeroSIS
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminPacienteEditViewModel model)
    {
        ViewData["Title"] = "Editar paciente";
        if (!ModelState.IsValid) return View(model);

        var paciente = await _context.Pacientes
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.IdPaciente == model.IdPaciente);

        if (paciente?.Usuario is null) return NotFound();

        var email = model.Correo.Trim();
        if (!email.Contains('@'))
        {
            email = $"{email}@siscitasweb.local";
        }
        email = email.ToLowerInvariant();

        var existsOtherEmail = await _context.Usuarios.AnyAsync(u => u.Correo.ToLower() == email && u.IdUsuario != paciente.IdUsuario);
        if (existsOtherEmail)
        {
            ModelState.AddModelError(nameof(AdminPacienteEditViewModel.Correo), "El correo ya está registrado.");
            model.NumeroHistoriaClinica = paciente.NumeroSIS;
            return View(model);
        }

        var existsOtherDni = await _context.Pacientes.AnyAsync(p => p.DNI == model.DNI && p.IdPaciente != paciente.IdPaciente);
        if (existsOtherDni)
        {
            ModelState.AddModelError(nameof(AdminPacienteEditViewModel.DNI), "El DNI ya está registrado.");
            model.NumeroHistoriaClinica = paciente.NumeroSIS;
            return View(model);
        }

        paciente.Usuario.Correo = email;
        paciente.Usuario.Estado = model.Estado;

        paciente.DNI = model.DNI;
        paciente.Nombres = model.Nombres;
        paciente.ApellidoPaterno = model.ApellidoPaterno;
        paciente.ApellidoMaterno = model.ApellidoMaterno;
        paciente.FechaNacimiento = model.FechaNacimiento;
        paciente.Sexo = model.Sexo;
        paciente.Telefono = model.Telefono;
        paciente.Direccion = model.Direccion;
        paciente.Estado = model.Estado;

        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Paciente actualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo actualizar el paciente.";
            model.NumeroHistoriaClinica = paciente.NumeroSIS;
            return View(model);
        }
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
