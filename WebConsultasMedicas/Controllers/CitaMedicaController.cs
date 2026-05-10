using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;
using WebConsultasMedicas.Models.Citas;

namespace WebConsultasMedicas.Controllers;

[Authorize(Roles = "Administrador")]
public class CitaMedicaController : Controller
{
    private readonly ApplicationDbContext _context;

    public CitaMedicaController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Citas médicas";
        var items = await _context.CitasMedicas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .Include(c => c.Especialidad)
            .Include(c => c.EstadoCita)
            .AsNoTracking()
            .OrderByDescending(c => c.FechaCita)
            .ThenByDescending(c => c.HoraCita)
            .Take(200)
            .ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Create(DateOnly? fechaCita, int? idPaciente, int? idEspecialidad, int? idMedico)
    {
        ViewData["Title"] = "Nueva cita";

        var vm = new CitaNuevaViewModel
        {
            FechaCita = fechaCita ?? DateOnly.FromDateTime(DateTime.Today),
            IdPaciente = idPaciente,
            IdEspecialidad = idEspecialidad,
            IdMedico = idMedico
        };

        await LoadCombosAsync(vm);

        // Si aún no hay paciente seleccionado, toma el primero para agilizar (admin).
        if (!vm.IdPaciente.HasValue && vm.Pacientes.Any())
        {
            vm.IdPaciente = vm.Pacientes.Select(i => int.TryParse(i.Value, out var v) ? v : 0).FirstOrDefault(v => v > 0);
            await LoadCombosAsync(vm);
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CitaNuevaViewModel model)
    {
        ViewData["Title"] = "Nueva cita";
        await LoadCombosAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var horario = await _context.HorariosMedicos.AsNoTracking()
            .Include(h => h.Medico)
            .Include(h => h.Especialidad)
            .FirstOrDefaultAsync(h =>
                h.IdHorarioMedico == model.IdHorarioMedico &&
                h.Estado &&
                h.Fecha == model.FechaCita &&
                h.IdMedico == model.IdMedico &&
                h.IdEspecialidad == model.IdEspecialidad);

        if (horario is null)
        {
            ModelState.AddModelError(nameof(CitaNuevaViewModel.IdHorarioMedico), "Horario inválido para la fecha/médico seleccionados.");
            return View(model);
        }

        var item = new CitaMedica
        {
            IdPaciente = model.IdPaciente.GetValueOrDefault(),
            IdHorarioMedico = horario.IdHorarioMedico,
            IdMedico = horario.IdMedico,
            IdEspecialidad = horario.IdEspecialidad,
            IdEstadoCita = 1, // Programada por defecto
            FechaCita = horario.Fecha,
            HoraCita = horario.HoraInicio,
            MotivoConsulta = null,
            Observacion = null
        };

        _context.CitasMedicas.Add(item);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cita registrada (Programada).";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo guardar. Verifica duplicados/horarios.";
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var item = await _context.CitasMedicas.FindAsync(id.Value);
        if (item is null) return NotFound();

        ViewData["Title"] = "Editar cita";
        await LoadLegacyCombosAsync(item.IdPaciente, item.IdHorarioMedico, item.IdEstadoCita);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdCita,IdPaciente,IdHorarioMedico,IdEstadoCita,FechaCita,HoraCita,MotivoConsulta,Observacion,FechaRegistro")] CitaMedica item)
    {
        if (id != item.IdCita) return NotFound();

        ViewData["Title"] = "Editar cita";
        await LoadLegacyCombosAsync(item.IdPaciente, item.IdHorarioMedico, item.IdEstadoCita);

        var horarioInfo = await _context.HorariosMedicos.AsNoTracking()
            .Where(h => h.IdHorarioMedico == item.IdHorarioMedico)
            .Select(h => new { h.IdMedico, h.IdEspecialidad, h.Fecha, h.HoraInicio })
            .FirstOrDefaultAsync();

        if (horarioInfo is null)
        {
            ModelState.AddModelError(nameof(CitaMedica.IdHorarioMedico), "Horario médico inválido.");
            return View(item);
        }

        item.IdMedico = horarioInfo.IdMedico;
        item.IdEspecialidad = horarioInfo.IdEspecialidad;
        item.FechaCita = horarioInfo.Fecha;
        item.HoraCita = horarioInfo.HoraInicio;
        ModelState.Remove(nameof(CitaMedica.IdMedico));
        ModelState.Remove(nameof(CitaMedica.IdEspecialidad));
        if (!ModelState.IsValid) return View(item);

        _context.Entry(item).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cita actualizada.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo guardar. Verifica duplicados/horarios.";
            return View(item);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var item = await _context.CitasMedicas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .Include(c => c.Especialidad)
            .Include(c => c.EstadoCita)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCita == id.Value);
        if (item is null) return NotFound();

        ViewData["Title"] = "Eliminar cita";
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.CitasMedicas.FindAsync(id);
        if (item is null) return RedirectToAction(nameof(Index));

        _context.CitasMedicas.Remove(item);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cita eliminada.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo eliminar. Puede estar relacionada a historial.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCombosAsync(CitaNuevaViewModel vm)
    {
        var pacientes = await _context.Pacientes.AsNoTracking()
            .Include(p => p.Usuario)
            .Where(p => p.Estado && p.Usuario.Estado)
            .OrderBy(p => p.ApellidoPaterno)
            .ThenBy(p => p.Nombres)
            .Select(p => new
            {
                p.IdPaciente,
                Nombre = $"{p.ApellidoPaterno} {p.ApellidoMaterno}, {p.Nombres} ({p.DNI})"
            })
            .ToListAsync();

        var especialidades = await _context.Especialidades.AsNoTracking()
            .Where(e => e.Estado)
            .OrderBy(e => e.Nombre)
            .ToListAsync();

        var medicos = Array.Empty<object>();
        if (vm.IdEspecialidad.HasValue && vm.IdEspecialidad.Value > 0)
        {
            medicos = await _context.Medicos.AsNoTracking()
                .Where(m => m.Estado && m.IdEspecialidad == vm.IdEspecialidad.Value)
                .OrderBy(m => m.ApellidoPaterno)
                .ThenBy(m => m.Nombres)
                .Select(m => new
                {
                    m.IdMedico,
                    Nombre = $"{m.ApellidoPaterno} {m.ApellidoMaterno}, {m.Nombres} (CMP: {m.CMP})"
                })
                .ToArrayAsync();
        }

        var horarios = Array.Empty<object>();
        if (vm.IdMedico.HasValue && vm.IdMedico.Value > 0)
        {
            // Horarios activos por fecha/médico, excluyendo ya reservados en estados activos
            var baseHorarios = _context.HorariosMedicos.AsNoTracking()
                .Where(h => h.Estado && h.IdMedico == vm.IdMedico.Value && h.Fecha == vm.FechaCita);

            var reservedHorarioIds = _context.CitasMedicas.AsNoTracking()
                .Where(c => c.IdMedico == vm.IdMedico.Value && c.FechaCita == vm.FechaCita && (c.IdEstadoCita == 1 || c.IdEstadoCita == 2 || c.IdEstadoCita == 4))
                .Select(c => c.IdHorarioMedico);

            // Evita IN/OPENJSON: trae horarios del día y filtra en memoria por los ids reservados.
            var reservedIds = await reservedHorarioIds.ToListAsync();
            var reservedSet = reservedIds.ToHashSet();

            var horariosList = await baseHorarios
                .OrderBy(h => h.HoraInicio)
                .Select(h => new
                {
                    h.IdHorarioMedico,
                    Nombre = $"{h.HoraInicio:hh\\:mm} - {h.HoraFin:hh\\:mm}"
                })
                .ToListAsync();

            horarios = horariosList
                .Where(h => !reservedSet.Contains(h.IdHorarioMedico))
                .ToArray();
        }

        vm.Pacientes = new SelectList(pacientes, "IdPaciente", "Nombre", vm.IdPaciente);
        vm.Especialidades = new SelectList(especialidades, nameof(Especialidad.IdEspecialidad), nameof(Especialidad.Nombre), vm.IdEspecialidad);
        vm.Medicos = new SelectList(medicos, "IdMedico", "Nombre", vm.IdMedico);
        vm.Horarios = new SelectList(horarios, "IdHorarioMedico", "Nombre", vm.IdHorarioMedico);
    }

    // Mantiene combos antiguos para Edit/Delete.
    private async Task LoadLegacyCombosAsync(int? selectedPaciente = null, int? selectedHorario = null, int? selectedEstado = null)
    {
        var pacientes = await _context.Pacientes.AsNoTracking()
            .OrderBy(p => p.ApellidoPaterno)
            .ThenBy(p => p.Nombres)
            .Select(p => new
            {
                p.IdPaciente,
                Nombre = $"{p.ApellidoPaterno} {p.ApellidoMaterno}, {p.Nombres} ({p.DNI})"
            })
            .ToListAsync();

        var horarios = await _context.HorariosMedicos.AsNoTracking()
            .Include(h => h.Medico)
            .Include(h => h.Especialidad)
            .OrderBy(h => h.IdHorarioMedico)
            .Select(h => new
            {
                h.IdHorarioMedico,
                Nombre = $"{h.IdHorarioMedico} - {h.Medico.ApellidoPaterno} {h.Medico.ApellidoMaterno}, {h.Medico.Nombres} / {h.Especialidad.Nombre} ({h.Fecha:yyyy-MM-dd} {h.HoraInicio:hh\\:mm}-{h.HoraFin:hh\\:mm})"
            })
            .ToListAsync();

        var estados = await _context.EstadosCita.AsNoTracking()
            .OrderBy(e => e.IdEstadoCita)
            .ToListAsync();

        ViewBag.Pacientes = new SelectList(pacientes, "IdPaciente", "Nombre", selectedPaciente);
        ViewBag.Horarios = new SelectList(horarios, "IdHorarioMedico", "Nombre", selectedHorario);
        ViewBag.Estados = new SelectList(estados, nameof(EstadoCita.IdEstadoCita), nameof(EstadoCita.Nombre), selectedEstado);
    }
}
