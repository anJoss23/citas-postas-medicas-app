using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebConsultasMedicas.Data;
using WebConsultasMedicas.Models;
using WebConsultasMedicas.Security;
using Microsoft.AspNetCore.Authorization;

namespace WebConsultasMedicas.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuarioController : Controller
{
    private readonly ApplicationDbContext _context;

    public UsuarioController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Usuarios";
        var items = await _context.Usuarios
            .Include(u => u.Rol)
            .AsNoTracking()
            .OrderBy(u => u.Correo)
            .ToListAsync();
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Nuevo usuario";
        await LoadRolesAsync();
        return View(new Usuario { Estado = true, FechaRegistro = DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string correo, string password, int idRol, bool estado)
    {
        ViewData["Title"] = "Nuevo usuario";

        if (string.IsNullOrWhiteSpace(correo))
        {
            ModelState.AddModelError(nameof(Usuario.Correo), "El correo es obligatorio.");
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("Password", "La contraseña es obligatoria.");
        }

        var usuario = new Usuario
        {
            Correo = correo?.Trim() ?? string.Empty,
            ClaveHash = string.IsNullOrWhiteSpace(password) ? string.Empty : PasswordHasher.Sha256Hex(password),
            IdRol = idRol,
            Estado = estado,
            FechaRegistro = DateTime.Now
        };

        await LoadRolesAsync(idRol);
        if (!ModelState.IsValid)
        {
            return View(usuario);
        }

        _context.Usuarios.Add(usuario);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Usuario registrado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Usuario.Correo), "No se pudo guardar. Verifica que el correo no esté duplicado.");
            return View(usuario);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var usuario = await _context.Usuarios.FindAsync(id.Value);
        if (usuario is null) return NotFound();

        ViewData["Title"] = "Editar usuario";
        await LoadRolesAsync(usuario.IdRol);
        ViewBag.ResetPasswordHint = "Deja la contraseña vacía si no deseas cambiarla.";
        return View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string correo, string? password, int idRol, bool estado)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        ViewData["Title"] = "Editar usuario";
        ViewBag.ResetPasswordHint = "Deja la contraseña vacía si no deseas cambiarla.";

        usuario.Correo = (correo ?? string.Empty).Trim();
        usuario.IdRol = idRol;
        usuario.Estado = estado;

        if (!string.IsNullOrWhiteSpace(password))
        {
            usuario.ClaveHash = PasswordHasher.Sha256Hex(password);
        }

        if (string.IsNullOrWhiteSpace(usuario.Correo))
        {
            ModelState.AddModelError(nameof(Usuario.Correo), "El correo es obligatorio.");
        }

        await LoadRolesAsync(idRol);
        if (!ModelState.IsValid)
        {
            return View(usuario);
        }

        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Usuario actualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(Usuario.Correo), "No se pudo guardar. Verifica que el correo no esté duplicado.");
            return View(usuario);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUsuario == id.Value);
        if (usuario is null) return NotFound();

        ViewData["Title"] = "Eliminar usuario";
        return View(usuario);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null) return RedirectToAction(nameof(Index));

        _context.Usuarios.Remove(usuario);
        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Usuario eliminado.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se pudo eliminar. Puede estar relacionado a pacientes.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadRolesAsync(int? selectedId = null)
    {
        var roles = await _context.Roles.AsNoTracking().OrderBy(r => r.Nombre).ToListAsync();
        ViewBag.Roles = new SelectList(roles, nameof(Rol.IdRol), nameof(Rol.Nombre), selectedId);
    }
}
