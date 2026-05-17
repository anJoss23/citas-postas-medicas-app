using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Hubs;
using WebConsultasMedicas.Models;
using Microsoft.AspNetCore.SignalR;

namespace WebConsultasMedicas.Controllers;

[Authorize(Roles = "Medico,Administrador")]
public class MedicoPortalController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<CitasHub> _hub;

    public MedicoPortalController(ApplicationDbContext context, IHubContext<CitasHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    public async Task<IActionResult> MisCitas()
    {
        ViewData["Title"] = "Mis citas (Médico)";

        var idUsuario = GetUsuarioId();
        if (idUsuario is null) return Forbid();

        var idMedico = await _context.Medicos.AsNoTracking()
            .Where(m => m.IdUsuario == idUsuario.Value)
            .Select(m => m.IdMedico)
            .FirstOrDefaultAsync();

        if (idMedico == 0)
        {
            TempData["Error"] = "Tu usuario no está vinculado a un médico. Pide al admin que lo asigne.";
            return RedirectToAction("Login", "Auth");
        }

        var items = await _context.CitasMedicas.AsNoTracking()
            .Include(c => c.Paciente)
            .Include(c => c.Especialidad)
            .Include(c => c.EstadoCita)
            .Where(c => c.IdMedico == idMedico)
            .OrderByDescending(c => c.FechaCita)
            .ThenByDescending(c => c.HoraCita)
            .Take(200)
            .ToListAsync();

        ViewBag.IdMedico = idMedico;

        // Token simple para detectar cambios (fallback al realtime si SignalR no conecta).
        var token = await _context.CitasMedicas.AsNoTracking()
            .Where(c => c.IdMedico == idMedico)
            .Select(c => (DateTime?)c.FechaRegistro)
            .MaxAsync();

        var tokenHist = await _context.HistorialCitas.AsNoTracking()
            .Where(h => h.CitaMedica.IdMedico == idMedico)
            .Select(h => (DateTime?)h.FechaCambio)
            .MaxAsync();

        var last = token is null ? tokenHist : tokenHist is null ? token : (token > tokenHist ? token : tokenHist);
        ViewBag.UpdateToken = (last ?? DateTime.MinValue).ToString("O");

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> UpdateToken()
    {
        var idUsuario = GetUsuarioId();
        if (idUsuario is null) return Forbid();

        var idMedico = await _context.Medicos.AsNoTracking()
            .Where(m => m.IdUsuario == idUsuario.Value)
            .Select(m => m.IdMedico)
            .FirstOrDefaultAsync();

        if (idMedico == 0) return Forbid();

        var token = await _context.CitasMedicas.AsNoTracking()
            .Where(c => c.IdMedico == idMedico)
            .Select(c => (DateTime?)c.FechaRegistro)
            .MaxAsync();

        var tokenHist = await _context.HistorialCitas.AsNoTracking()
            .Where(h => h.CitaMedica.IdMedico == idMedico)
            .Select(h => (DateTime?)h.FechaCambio)
            .MaxAsync();

        var last = token is null ? tokenHist : tokenHist is null ? token : (token > tokenHist ? token : tokenHist);
        var value = (last ?? DateTime.MinValue).ToString("O");
        return Json(new { token = value });
    }

    public async Task<IActionResult> Atender(int? id)
    {
        if (id is null) return NotFound();

        var idUsuario = GetUsuarioId();
        if (idUsuario is null) return Forbid();

        var idMedico = await _context.Medicos.AsNoTracking()
            .Where(m => m.IdUsuario == idUsuario.Value)
            .Select(m => m.IdMedico)
            .FirstOrDefaultAsync();

        var cita = await _context.CitasMedicas.AsNoTracking()
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .Include(c => c.Especialidad)
            .Include(c => c.EstadoCita)
            .FirstOrDefaultAsync(c => c.IdCita == id.Value && c.IdMedico == idMedico);

        if (cita is null) return NotFound();

        var estados = await _context.EstadosCita.AsNoTracking().OrderBy(e => e.IdEstadoCita).ToListAsync();

        DateTime? fechaAtencion = null;
        string? observacionAtencion = null;
        var isReadOnly = cita.IdEstadoCita == 2; // Atendida
        if (isReadOnly)
        {
            var last = await _context.HistorialCitas.AsNoTracking()
                .Where(h => h.IdCita == cita.IdCita)
                .OrderByDescending(h => h.FechaCambio)
                .Select(h => new { h.FechaCambio, h.Observacion })
                .FirstOrDefaultAsync();
            fechaAtencion = last?.FechaCambio;
            observacionAtencion = last?.Observacion;
        }

        var model = new MedicoIndicacionViewModel
        {
            IdCita = cita.IdCita,
            Cita = cita,
            IdEstadoCita = cita.IdEstadoCita,
            IsReadOnly = isReadOnly,
            FechaAtencion = fechaAtencion,
            Observacion = observacionAtencion,
            Estados = new SelectList(estados, nameof(EstadoCita.IdEstadoCita), nameof(EstadoCita.Nombre), cita.IdEstadoCita)
        };

        ViewData["Title"] = $"Cita #{cita.IdCita}";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Atender(MedicoIndicacionViewModel model)
    {
        var idUsuario = GetUsuarioId();
        if (idUsuario is null) return Forbid();

        var idMedico = await _context.Medicos.AsNoTracking()
            .Where(m => m.IdUsuario == idUsuario.Value)
            .Select(m => m.IdMedico)
            .FirstOrDefaultAsync();

        var cita = await _context.CitasMedicas.FirstOrDefaultAsync(c => c.IdCita == model.IdCita && c.IdMedico == idMedico);
        if (cita is null) return NotFound();

        var estados = await _context.EstadosCita.AsNoTracking().OrderBy(e => e.IdEstadoCita).ToListAsync();
        model.Estados = new SelectList(estados, nameof(EstadoCita.IdEstadoCita), nameof(EstadoCita.Nombre), model.IdEstadoCita);

        if (cita.IdEstadoCita == 2)
        {
            TempData["Error"] = "Esta cita ya fue atendida y no puede modificarse.";
            return RedirectToAction(nameof(Atender), new { id = cita.IdCita });
        }

        if (!ModelState.IsValid)
        {
            model.Cita = await _context.CitasMedicas.AsNoTracking()
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                .Include(c => c.Especialidad)
                .Include(c => c.EstadoCita)
                .FirstOrDefaultAsync(c => c.IdCita == model.IdCita);
            return View(model);
        }

        cita.IdEstadoCita = model.IdEstadoCita;

        _context.HistorialCitas.Add(new HistorialCita
        {
            IdCita = cita.IdCita,
            IdEstadoCita = model.IdEstadoCita,
            FechaCambio = DateTime.Now,
            Observacion = model.Observacion,
            UsuarioAccion = User.Identity?.Name
        });

        try
        {
            await _context.SaveChangesAsync();
            await _hub.Clients.Group(CitasHub.GroupForMedico(cita.IdMedico)).SendAsync("citasUpdated");
            TempData["Success"] = model.IdEstadoCita == 2 ? "Cita atendida y cerrada." : "Indicaciones/estado guardados.";
            return RedirectToAction(nameof(MisCitas));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo guardar las indicaciones.";
            model.Cita = await _context.CitasMedicas.AsNoTracking()
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                .Include(c => c.Especialidad)
                .Include(c => c.EstadoCita)
                .FirstOrDefaultAsync(c => c.IdCita == model.IdCita);
            return View(model);
        }
    }

    private int? GetUsuarioId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claim, out var id))
        {
            return id;
        }
        return null;
    }
}
