using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Models;
using WebConsultasMedicas.Security;

namespace WebConsultasMedicas.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();

        await EnsureRolesAsync(context);
        await EnsureEstadosCitaAsync(context);
        await EnsureDemoAccountsAsync(context);
    }

    private static async Task EnsureRolesAsync(ApplicationDbContext context)
    {
        var required = new[] { "Administrador", "Paciente", "Medico" };
        var existing = await context.Roles.AsNoTracking().Select(r => r.Nombre).ToListAsync();

        var missing = required.Where(r => !existing.Contains(r)).ToList();
        if (missing.Count == 0) return;

        foreach (var name in missing)
        {
            context.Roles.Add(new Rol { Nombre = name, Estado = true });
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureEstadosCitaAsync(ApplicationDbContext context)
    {
        var required = new[] { "Programada", "Atendida", "Cancelada", "Reprogramada", "No Asistio" };
        var existing = await context.EstadosCita.AsNoTracking().Select(e => e.Nombre).ToListAsync();

        var missing = required.Where(r => !existing.Contains(r)).ToList();
        if (missing.Count == 0) return;

        foreach (var name in missing)
        {
            context.EstadosCita.Add(new EstadoCita { Nombre = name });
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureDemoAccountsAsync(ApplicationDbContext context)
    {
        var adminRoleId = await context.Roles.Where(r => r.Nombre == "Administrador").Select(r => r.IdRol).FirstAsync();
        var pacienteRoleId = await context.Roles.Where(r => r.Nombre == "Paciente").Select(r => r.IdRol).FirstAsync();
        var medicoRoleId = await context.Roles.Where(r => r.Nombre == "Medico").Select(r => r.IdRol).FirstAsync();

        await EnsureUserAsync(context, "admin@siscitasweb.local", "admin123", adminRoleId);

        var pacienteUserId = await EnsureUserAsync(context, "paciente1@siscitasweb.local", "paciente123", pacienteRoleId);
        if (!await context.Pacientes.AnyAsync(p => p.IdUsuario == pacienteUserId))
        {
            var dni = "70000001";
            var sis = "HCLIN00001";
            if (await context.Pacientes.AnyAsync(p => p.DNI == dni))
            {
                dni = DateTime.Now.ToString("HHmmssff").PadLeft(8, '0')[..8];
            }
            if (await context.Pacientes.AnyAsync(p => p.NumeroSIS == sis))
            {
                sis = await NumeroHistoriaClinicaGenerator.NextAsync(context);
            }

            context.Pacientes.Add(new Paciente
            {
                IdUsuario = pacienteUserId,
                DNI = dni,
                Nombres = "Paciente",
                ApellidoPaterno = "Demo",
                ApellidoMaterno = "Uno",
                FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                Sexo = "M",
                Telefono = "999999999",
                Direccion = "Lima",
                NumeroSIS = sis,
                Estado = true
            });
            await context.SaveChangesAsync();
        }

        var medicoUserId = await EnsureUserAsync(context, "doctor1@siscitasweb.local", "doctor123", medicoRoleId);

        var especialidadId = await context.Especialidades
            .Where(e => e.Estado)
            .OrderBy(e => e.IdEspecialidad)
            .Select(e => e.IdEspecialidad)
            .FirstOrDefaultAsync();

        if (especialidadId == 0)
        {
            var especialidad = new Especialidad { Nombre = "Medicina General", Descripcion = "Atención general", Estado = true };
            context.Especialidades.Add(especialidad);
            await context.SaveChangesAsync();
            especialidadId = especialidad.IdEspecialidad;
        }

        if (!await context.Medicos.AnyAsync(m => m.IdUsuario == medicoUserId))
        {
            var cmp = "CMP7000001";
            if (await context.Medicos.AnyAsync(m => m.CMP == cmp))
            {
                cmp = $"CMP{DateTime.Now:HHmmss}";
            }

            context.Medicos.Add(new Medico
            {
                IdUsuario = medicoUserId,
                IdEspecialidad = especialidadId,
                CMP = cmp,
                Nombres = "Doctor",
                ApellidoPaterno = "Demo",
                ApellidoMaterno = "Uno",
                Telefono = "988888888",
                Correo = "doctor1@siscitasweb.local",
                Estado = true
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task<int> EnsureUserAsync(ApplicationDbContext context, string email, string password, int roleId)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var existing = await context.Usuarios.FirstOrDefaultAsync(u => u.Correo.ToLower() == normalized);
        if (existing is not null)
        {
            return existing.IdUsuario;
        }

        var user = new Usuario
        {
            Correo = normalized,
            ClaveHash = PasswordHasher.Sha256Hex(password),
            IdRol = roleId,
            Estado = true,
            FechaRegistro = DateTime.Now
        };

        context.Usuarios.Add(user);
        await context.SaveChangesAsync();
        return user.IdUsuario;
    }
}
