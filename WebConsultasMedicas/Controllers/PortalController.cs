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

[Authorize(Roles = "Paciente,Administrador")]
public class PortalController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<CitasHub> _hub;

    public PortalController(ApplicationDbContext context, IHubContext<CitasHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    public async Task<IActionResult> Buscar(int? idEspecialidad, string? medico)
    {
        ViewData["Title"] = "Agendar cita";

        var especialidades = await _context.Especialidades.AsNoTracking()
            .Where(e => e.Estado)
            .OrderBy(e => e.Nombre)
            .ToListAsync();

        var medicoQuery = medico?.Trim();
        var medicosQuery = _context.Medicos.AsNoTracking()
            .Include(m => m.Especialidad)
            .Where(m => m.Estado && m.Especialidad.Estado);

        if (idEspecialidad.HasValue)
        {
            medicosQuery = medicosQuery.Where(m => m.IdEspecialidad == idEspecialidad.Value);
        }

        if (!string.IsNullOrWhiteSpace(medicoQuery))
        {
            medicosQuery = medicosQuery.Where(m =>
                (m.Nombres + " " + m.ApellidoPaterno + " " + m.ApellidoMaterno).Contains(medicoQuery));
        }

        var medicos = await medicosQuery
            .OrderBy(m => m.Especialidad.Nombre)
            .ThenBy(m => m.ApellidoPaterno)
            .ThenBy(m => m.Nombres)
            .ToListAsync();

        return View(new PortalBuscarMedicosViewModel
        {
            IdEspecialidad = idEspecialidad,
            MedicoQuery = medicoQuery,
            Medicos = medicos,
            Especialidades = new SelectList(especialidades, nameof(Especialidad.IdEspecialidad), nameof(Especialidad.Nombre), idEspecialidad)
        });
    }

    public async Task<IActionResult> Horarios(int idMedico)
    {
        ViewData["Title"] = "Horarios";

        var medico = await _context.Medicos.AsNoTracking()
            .Include(m => m.Especialidad)
            .FirstOrDefaultAsync(m => m.IdMedico == idMedico && m.Estado);

        if (medico is null)
        {
            return NotFound();
        }

        var reservedHorarioIds = _context.CitasMedicas.AsNoTracking()
            .Where(c => c.IdEstadoCita != 3) // 3=Cancelada
            .Select(c => c.IdHorarioMedico)
            .Distinct();

        var horarios = await _context.HorariosMedicos.AsNoTracking()
            .Include(h => h.Turno)
            .Where(h => h.IdMedico == idMedico && h.Estado)
            .Where(h => h.Fecha >= DateOnly.FromDateTime(DateTime.Today))
            .Where(h => !reservedHorarioIds.Contains(h.IdHorarioMedico))
            .OrderBy(h => h.Fecha)
            .ThenBy(h => h.HoraInicio)
            .ToListAsync();

        ViewBag.Medico = medico;
        return View(horarios);
    }

    public async Task<IActionResult> Reservar(int idHorarioMedico)
    {
        ViewData["Title"] = "Reservar cita";

        var horario = await _context.HorariosMedicos.AsNoTracking()
            .Include(h => h.Medico)
            .ThenInclude(m => m.Especialidad)
            .Include(h => h.Turno)
            .FirstOrDefaultAsync(h => h.IdHorarioMedico == idHorarioMedico);

        if (horario is null)
        {
            return NotFound();
        }

        var model = new PortalReservarViewModel
        {
            IdHorarioMedico = idHorarioMedico,
            FechaCita = horario.Fecha,
            HoraCita = horario.HoraInicio,
            Horario = horario
        };

        if (User.IsInRole("Administrador"))
        {
            var pacientes = await _context.Pacientes.AsNoTracking()
                .Include(p => p.Usuario)
                .Where(p => p.Estado && p.Usuario.Estado)
                .OrderBy(p => p.ApellidoPaterno)
                .ThenBy(p => p.ApellidoMaterno)
                .ThenBy(p => p.Nombres)
                .Select(p => new
                {
                    p.IdPaciente,
                    Nombre = $"{p.ApellidoPaterno} {p.ApellidoMaterno}, {p.Nombres} ({p.DNI})"
                })
                .ToListAsync();

            model.Pacientes = new SelectList(pacientes, "IdPaciente", "Nombre");
        }

        model.HorasDisponibles = BuildHorasSelectList(horario, model.HoraCita);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reservar(PortalReservarViewModel model)
    {
        ViewData["Title"] = "Reservar cita";

        var horario = await _context.HorariosMedicos.AsNoTracking()
            .Include(h => h.Medico)
            .ThenInclude(m => m.Especialidad)
            .Include(h => h.Turno)
            .FirstOrDefaultAsync(h => h.IdHorarioMedico == model.IdHorarioMedico);

        if (horario is null)
        {
            return NotFound();
        }

        model.Horario = horario;
        model.HorasDisponibles = BuildHorasSelectList(horario, model.HoraCita);

        int pacienteId;
        if (User.IsInRole("Administrador"))
        {
            if (model.IdPaciente is null || model.IdPaciente.Value <= 0)
            {
                ModelState.AddModelError(nameof(PortalReservarViewModel.IdPaciente), "Seleccione un paciente.");
            }
            else
            {
                var existsPaciente = await _context.Pacientes.AsNoTracking()
                    .Include(p => p.Usuario)
                    .AnyAsync(p => p.IdPaciente == model.IdPaciente.Value && p.Estado && p.Usuario.Estado);

                if (!existsPaciente)
                {
                    ModelState.AddModelError(nameof(PortalReservarViewModel.IdPaciente), "Paciente inválido.");
                }
            }

            var pacientes = await _context.Pacientes.AsNoTracking()
                .Include(p => p.Usuario)
                .Where(p => p.Estado && p.Usuario.Estado)
                .OrderBy(p => p.ApellidoPaterno)
                .ThenBy(p => p.ApellidoMaterno)
                .ThenBy(p => p.Nombres)
                .Select(p => new
                {
                    p.IdPaciente,
                    Nombre = $"{p.ApellidoPaterno} {p.ApellidoMaterno}, {p.Nombres} ({p.DNI})"
                })
                .ToListAsync();
            model.Pacientes = new SelectList(pacientes, "IdPaciente", "Nombre", model.IdPaciente);

            pacienteId = model.IdPaciente ?? 0;
        }
        else
        {
            var idUsuario = GetUsuarioId();
            if (idUsuario is null)
            {
                return Forbid();
            }

            pacienteId = await _context.Pacientes
                .Where(p => p.IdUsuario == idUsuario.Value)
                .Select(p => p.IdPaciente)
                .FirstOrDefaultAsync();

            if (pacienteId == 0)
            {
                TempData["Error"] = "No se encontró tu perfil de paciente.";
                return RedirectToAction(nameof(Buscar));
            }
        }

        if (model.FechaCita != horario.Fecha)
        {
            ModelState.AddModelError(nameof(PortalReservarViewModel.FechaCita), "La fecha no coincide con la fecha del horario.");
        }

        if (model.HoraCita != horario.HoraInicio)
        {
            ModelState.AddModelError(nameof(PortalReservarViewModel.HoraCita), "La hora no coincide con el inicio del bloque.");
        }

        var citasEnFecha = await _context.CitasMedicas.AsNoTracking()
            .CountAsync(c => c.IdHorarioMedico == horario.IdHorarioMedico
                             && c.FechaCita == model.FechaCita
                             && c.IdEstadoCita != 3); // 3=Cancelada

        if (citasEnFecha >= 1)
        {
            ModelState.AddModelError(string.Empty, "Ese bloque ya está reservado para esa fecha.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var cita = new CitaMedica
        {
            IdPaciente = pacienteId,
            IdHorarioMedico = horario.IdHorarioMedico,
            IdMedico = horario.IdMedico,
            IdEspecialidad = horario.IdEspecialidad,
            IdEstadoCita = 1, // Programada
            FechaCita = model.FechaCita,
            HoraCita = model.HoraCita,
            MotivoConsulta = model.MotivoConsulta,
            Observacion = model.Observacion
        };

        _context.CitasMedicas.Add(cita);
        try
        {
            await _context.SaveChangesAsync();
            await _hub.Clients.Group(CitasHub.GroupForMedico(cita.IdMedico)).SendAsync("citasUpdated");
            TempData["Success"] = "Cita registrada.";
            return RedirectToAction(nameof(MisCitas));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo registrar. Puede existir un conflicto de horario.";
            return View(model);
        }
    }

    public async Task<IActionResult> MisCitas()
    {
        ViewData["Title"] = "Mis citas";

        var idUsuario = GetUsuarioId();
        if (idUsuario is null) return Forbid();

        var pacienteId = await _context.Pacientes
            .Where(p => p.IdUsuario == idUsuario.Value)
            .Select(p => p.IdPaciente)
            .FirstOrDefaultAsync();

        if (pacienteId == 0)
        {
            TempData["Error"] = "No se encontró tu perfil de paciente.";
            return RedirectToAction(nameof(Buscar));
        }

        var items = await _context.CitasMedicas.AsNoTracking()
            .Include(c => c.Medico)
            .Include(c => c.Especialidad)
            .Include(c => c.EstadoCita)
            .Where(c => c.IdPaciente == pacienteId)
            .OrderByDescending(c => c.FechaCita)
            .ThenByDescending(c => c.HoraCita)
            .Take(200)
            .ToListAsync();

        return View(items);
    }

    public async Task<IActionResult> Detalle(int idCita)
    {
        ViewData["Title"] = "Detalle de cita";

        var idUsuario = GetUsuarioId();
        if (idUsuario is null) return Forbid();

        var pacienteId = await _context.Pacientes
            .Where(p => p.IdUsuario == idUsuario.Value)
            .Select(p => p.IdPaciente)
            .FirstOrDefaultAsync();

        if (pacienteId == 0)
        {
            TempData["Error"] = "No se encontró tu perfil de paciente.";
            return RedirectToAction(nameof(MisCitas));
        }

        var cita = await _context.CitasMedicas.AsNoTracking()
            .Include(c => c.Medico)
            .Include(c => c.Especialidad)
            .Include(c => c.EstadoCita)
            .FirstOrDefaultAsync(c => c.IdCita == idCita && c.IdPaciente == pacienteId);

        if (cita is null) return NotFound();

        var historial = await _context.HistorialCitas.AsNoTracking()
            .Include(h => h.EstadoCita)
            .Where(h => h.IdCita == cita.IdCita)
            .OrderByDescending(h => h.FechaCambio)
            .ToListAsync();

        ViewBag.Cita = cita;
        return View(historial);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int idCita)
    {
        var idUsuario = GetUsuarioId();
        if (idUsuario is null) return Forbid();

        var pacienteId = await _context.Pacientes
            .Where(p => p.IdUsuario == idUsuario.Value)
            .Select(p => p.IdPaciente)
            .FirstOrDefaultAsync();

        if (pacienteId == 0) return Forbid();

        var cita = await _context.CitasMedicas.FirstOrDefaultAsync(c => c.IdCita == idCita && c.IdPaciente == pacienteId);
        if (cita is null) return NotFound();

        if (cita.IdEstadoCita == 3)
        {
            TempData["Error"] = "La cita ya está cancelada.";
            return RedirectToAction(nameof(MisCitas));
        }

        cita.IdEstadoCita = 3; // Cancelada

        _context.HistorialCitas.Add(new HistorialCita
        {
            IdCita = cita.IdCita,
            IdEstadoCita = 3,
            FechaCambio = DateTime.Now,
            Observacion = "Cancelada por el paciente.",
            UsuarioAccion = User.Identity?.Name
        });

        try
        {
            await _context.SaveChangesAsync();
            await _hub.Clients.Group(CitasHub.GroupForMedico(cita.IdMedico)).SendAsync("citasUpdated");
            TempData["Success"] = "Cita cancelada.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo cancelar la cita.";
        }

        return RedirectToAction(nameof(MisCitas));
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

    private static SelectList BuildHorasSelectList(HorarioMedico horario, TimeOnly selected)
    {
        var value = horario.HoraInicio.ToString("HH:mm");
        return new SelectList(new[] { value }, value);
    }
}
