using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;
using WebConsultasMedicas.Models.Reportes;

namespace WebConsultasMedicas.Controllers;

[Authorize(Roles = "Administrador")]
public class ReportesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> AtencionesPorMedico(int? idEspecialidad, int? idMedico)
    {
        ViewData["Title"] = "Atenciones por médico";

        var especialidades = await _context.Especialidades.AsNoTracking()
            .Where(e => e.Estado)
            .OrderBy(e => e.Nombre)
            .ToListAsync();

        var medicosQuery = _context.Medicos.AsNoTracking()
            .Include(m => m.Especialidad)
            .Where(m => m.Estado && m.Especialidad.Estado);

        if (idEspecialidad.HasValue)
        {
            medicosQuery = medicosQuery.Where(m => m.IdEspecialidad == idEspecialidad.Value);
        }

        var medicos = await medicosQuery
            .OrderBy(m => m.ApellidoPaterno)
            .ThenBy(m => m.Nombres)
            .Select(m => new
            {
                m.IdMedico,
                Nombre = $"{m.ApellidoPaterno} {m.ApellidoMaterno}, {m.Nombres} ({m.Especialidad.Nombre})"
            })
            .ToListAsync();

        var rows = await BuildAtencionesRowsAsync(idEspecialidad, idMedico);

        return View(new AtencionesPorMedicoFilterViewModel
        {
            IdEspecialidad = idEspecialidad,
            IdMedico = idMedico,
            Especialidades = new SelectList(especialidades, nameof(Especialidad.IdEspecialidad), nameof(Especialidad.Nombre), idEspecialidad),
            Medicos = new SelectList(medicos, "IdMedico", "Nombre", idMedico),
            Rows = rows
        });
    }

    [HttpGet]
    public async Task<IActionResult> AtencionesPorMedicoExcel(int? idEspecialidad, int? idMedico)
    {
        var rows = await BuildAtencionesRowsAsync(idEspecialidad, idMedico);

        var csv = new StringBuilder();
        AppendCsvRow(csv, "IdCita", "Fecha", "Hora", "Especialidad", "Médico", "Paciente", "Estado", "ÚltimoCambio", "ÚltimaObservación");

        foreach (var r in rows)
        {
            AppendCsvRow(
                csv,
                r.IdCita.ToString(),
                r.FechaCita.ToString("yyyy-MM-dd"),
                r.HoraCita.ToString("HH:mm"),
                r.Especialidad,
                r.Medico,
                r.Paciente,
                r.Estado,
                r.UltimoCambio?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
                r.UltimaObservacion ?? string.Empty
            );
        }

        var bytes = ToUtf8BomBytes(csv.ToString());
        return File(bytes, "text/csv; charset=utf-8", "AtencionesPorMedico.csv");
    }

    public async Task<IActionResult> HistorialPaciente(int? idPaciente)
    {
        ViewData["Title"] = "Historial del paciente";

        var pacientes = await _context.Pacientes.AsNoTracking()
            .OrderBy(p => p.ApellidoPaterno)
            .ThenBy(p => p.ApellidoMaterno)
            .ThenBy(p => p.Nombres)
            .Select(p => new
            {
                p.IdPaciente,
                Nombre = $"{p.ApellidoPaterno} {p.ApellidoMaterno}, {p.Nombres} ({p.DNI})"
            })
            .ToListAsync();

        var rows = idPaciente.HasValue
            ? await BuildHistorialPacienteRowsAsync(idPaciente.Value)
            : Array.Empty<HistorialPacienteRow>();

        return View(new HistorialPacienteViewModel
        {
            IdPaciente = idPaciente,
            Pacientes = new SelectList(pacientes, "IdPaciente", "Nombre", idPaciente),
            Rows = rows
        });
    }

    [HttpGet]
    public async Task<IActionResult> HistorialPacienteExcel(int idPaciente)
    {
        var rows = await BuildHistorialPacienteRowsAsync(idPaciente);

        var csv = new StringBuilder();
        AppendCsvRow(csv, "IdCita", "FechaCita", "HoraCita", "Especialidad", "Médico", "FechaCambio", "Estado", "Observación", "UsuarioAcción");

        foreach (var r in rows)
        {
            AppendCsvRow(
                csv,
                r.IdCita.ToString(),
                r.FechaCita.ToString("yyyy-MM-dd"),
                r.HoraCita.ToString("HH:mm"),
                r.Especialidad,
                r.Medico,
                r.FechaCambio.ToString("yyyy-MM-dd HH:mm:ss"),
                r.Estado,
                r.Observacion ?? string.Empty,
                r.UsuarioAccion ?? string.Empty
            );
        }

        var bytes = ToUtf8BomBytes(csv.ToString());
        return File(bytes, "text/csv; charset=utf-8", "HistorialPaciente.csv");
    }

    private static byte[] ToUtf8BomBytes(string text)
    {
        // Excel (Windows) opens UTF-8 CSV more reliably with BOM.
        var utf8Bom = Encoding.UTF8.GetPreamble();
        var payload = Encoding.UTF8.GetBytes(text);
        var bytes = new byte[utf8Bom.Length + payload.Length];
        Buffer.BlockCopy(utf8Bom, 0, bytes, 0, utf8Bom.Length);
        Buffer.BlockCopy(payload, 0, bytes, utf8Bom.Length, payload.Length);
        return bytes;
    }

    private static void AppendCsvRow(StringBuilder sb, params string[] columns)
    {
        static string Escape(string value)
        {
            var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
            if (value.Contains('"'))
            {
                value = value.Replace("\"", "\"\"");
            }
            return needsQuotes ? $"\"{value}\"" : value;
        }

        for (var i = 0; i < columns.Length; i++)
        {
            if (i > 0) sb.Append(',');
            sb.Append(Escape(columns[i] ?? string.Empty));
        }
        sb.AppendLine();
    }

    private async Task<IReadOnlyList<AtencionPorMedicoRow>> BuildAtencionesRowsAsync(int? idEspecialidad, int? idMedico)
    {
        var baseQuery = _context.CitasMedicas.AsNoTracking()
            .Where(c => c.IdEstadoCita == 2); // Atendida

        if (idEspecialidad.HasValue)
        {
            baseQuery = baseQuery.Where(c => c.IdEspecialidad == idEspecialidad.Value);
        }

        if (idMedico.HasValue)
        {
            baseQuery = baseQuery.Where(c => c.IdMedico == idMedico.Value);
        }

        var items = await baseQuery
            .Include(c => c.Medico)
            .Include(c => c.Paciente)
            .Include(c => c.Especialidad)
            .Include(c => c.EstadoCita)
            .OrderByDescending(c => c.FechaCita)
            .ThenByDescending(c => c.HoraCita)
            .Take(2000)
            .ToListAsync();

        // Evita "OPENJSON ... WITH" (requiere compatibilidad SQL Server 2016+).
        // En vez de usar Contains sobre una lista en memoria, filtramos con un subquery.
        var citaIdQuery = baseQuery.Select(c => c.IdCita);

        var historial = await _context.HistorialCitas.AsNoTracking()
            .Where(h => citaIdQuery.Contains(h.IdCita))
            .OrderByDescending(h => h.FechaCambio)
            .ToListAsync();

        var lastByCita = new Dictionary<int, HistorialCita>();
        foreach (var h in historial)
        {
            if (!lastByCita.ContainsKey(h.IdCita))
            {
                lastByCita[h.IdCita] = h;
            }
        }

        return items.Select(c =>
        {
            lastByCita.TryGetValue(c.IdCita, out var h);
            return new AtencionPorMedicoRow
            {
                IdCita = c.IdCita,
                FechaCita = c.FechaCita,
                HoraCita = c.HoraCita,
                Especialidad = c.Especialidad?.Nombre ?? string.Empty,
                Medico = $"{c.Medico?.ApellidoPaterno} {c.Medico?.ApellidoMaterno}, {c.Medico?.Nombres}",
                Paciente = $"{c.Paciente?.ApellidoPaterno} {c.Paciente?.ApellidoMaterno}, {c.Paciente?.Nombres} ({c.Paciente?.DNI})",
                Estado = c.EstadoCita?.Nombre ?? string.Empty,
                UltimaObservacion = h?.Observacion,
                UltimoCambio = h?.FechaCambio
            };
        }).ToList();
    }

    private async Task<IReadOnlyList<HistorialPacienteRow>> BuildHistorialPacienteRowsAsync(int idPaciente)
    {
        var query = _context.HistorialCitas.AsNoTracking()
            .Include(h => h.EstadoCita)
            .Include(h => h.CitaMedica)
                .ThenInclude(c => c.Medico)
            .Include(h => h.CitaMedica)
                .ThenInclude(c => c.Especialidad)
            .Where(h => h.CitaMedica.IdPaciente == idPaciente);

        var items = await query
            .OrderByDescending(h => h.FechaCambio)
            .Take(5000)
            .ToListAsync();

        return items.Select(h =>
        {
            var c = h.CitaMedica;
            return new HistorialPacienteRow
            {
                IdCita = c.IdCita,
                FechaCita = c.FechaCita,
                HoraCita = c.HoraCita,
                Especialidad = c.Especialidad?.Nombre ?? string.Empty,
                Medico = $"{c.Medico?.ApellidoPaterno} {c.Medico?.ApellidoMaterno}, {c.Medico?.Nombres}",
                FechaCambio = h.FechaCambio,
                Estado = h.EstadoCita?.Nombre ?? string.Empty,
                Observacion = h.Observacion,
                UsuarioAccion = h.UsuarioAccion
            };
        }).ToList();
    }
}
