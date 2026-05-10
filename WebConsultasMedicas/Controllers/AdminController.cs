using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models.Dashboard;

namespace WebConsultasMedicas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Inicio()
        {
            ViewData["Title"] = "Inicio";

            var today = DateOnly.FromDateTime(DateTime.Today);
            var next7 = today.AddDays(7);

            var totalUsuarios = await _context.Usuarios.AsNoTracking().CountAsync();
            var totalPacientes = await _context.Pacientes.AsNoTracking().CountAsync();
            var totalMedicos = await _context.Medicos.AsNoTracking().CountAsync();

            var citasHoy = await _context.CitasMedicas.AsNoTracking()
                .CountAsync(c => c.FechaCita == today);

            var citasProx7Dias = await _context.CitasMedicas.AsNoTracking()
                .CountAsync(c => c.FechaCita >= today && c.FechaCita <= next7);

            var programadas = await _context.CitasMedicas.AsNoTracking().CountAsync(c => c.IdEstadoCita == 1);
            var atendidas = await _context.CitasMedicas.AsNoTracking().CountAsync(c => c.IdEstadoCita == 2);
            var canceladas = await _context.CitasMedicas.AsNoTracking().CountAsync(c => c.IdEstadoCita == 3);
            var reprogramadas = await _context.CitasMedicas.AsNoTracking().CountAsync(c => c.IdEstadoCita == 4);
            var noAsistio = await _context.CitasMedicas.AsNoTracking().CountAsync(c => c.IdEstadoCita == 5);

            var topEspecialidades = await _context.CitasMedicas.AsNoTracking()
                .Include(c => c.Especialidad)
                .GroupBy(c => c.Especialidad.Nombre)
                .Select(g => new TopEspecialidadMetric
                {
                    Especialidad = g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync();

            return View(new AdminDashboardViewModel
            {
                TotalUsuarios = totalUsuarios,
                TotalPacientes = totalPacientes,
                TotalMedicos = totalMedicos,
                CitasHoy = citasHoy,
                CitasProx7Dias = citasProx7Dias,
                Programadas = programadas,
                Atendidas = atendidas,
                Canceladas = canceladas,
                Reprogramadas = reprogramadas,
                NoAsistio = noAsistio,
                TopEspecialidades = topEspecialidades
            });
        }

        public IActionResult Dashboard()
        {
            return RedirectToAction(nameof(Inicio));
        }
    }
}
