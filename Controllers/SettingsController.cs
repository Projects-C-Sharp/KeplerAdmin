using admin_tickets.Models;
using admin_tickets.Services;
using Microsoft.AspNetCore.Mvc;

namespace admin_tickets.Controllers;

public class SettingsController : Controller
{
    private readonly ApiService _api;
    public SettingsController(ApiService api) => _api = api;

    private bool IsAuthenticated =>
        HttpContext.Session.GetString("AccessToken") != null;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");
        var profile = await _api.GetProfileAsync();

        // Persist photo & name in session so sidebar always shows them
        if (profile?.PhotoUrl != null)
            HttpContext.Session.SetString("UserPhoto", profile.PhotoUrl);
        if (profile?.FullName != null)
            HttpContext.Session.SetString("UserFullName", profile.FullName);

        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            TempData["PasswordError"] = "Las contraseñas nuevas no coinciden.";
            return RedirectToAction("Index");
        }

        var ok = await _api.ChangePasswordAsync(dto);
        TempData[ok ? "PasswordSuccess" : "PasswordError"] = ok
            ? "Contraseña actualizada correctamente."
            : "No se pudo cambiar la contraseña. Verifica la contraseña actual.";

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto(IFormFile photo)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Auth");

        if (photo == null || photo.Length == 0)
        {
            TempData["PhotoError"] = "Selecciona una imagen válida.";
            return RedirectToAction("Index");
        }

        var photoUrl = await _api.UploadProfilePhotoAsync(photo);
        if (photoUrl != null)
        {
            HttpContext.Session.SetString("UserPhoto", photoUrl);
            TempData["PhotoSuccess"] = "Foto actualizada correctamente.";
        }
        else
        {
            TempData["PhotoError"] = "No se pudo actualizar la foto. Intenta de nuevo.";
        }

        return RedirectToAction("Index");
    }
}
