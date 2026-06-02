using admin_tickets.Models;
using admin_tickets.Services;
using Microsoft.AspNetCore.Mvc;

namespace admin_tickets.Controllers;

public class ShowtimesController : Controller
{
    private readonly ApiService _api;
    public ShowtimesController(ApiService api) => _api = api;

    private IActionResult? RequireAuth() =>
        HttpContext.Session.GetString("AccessToken") == null
            ? RedirectToAction("Login", "Auth") : null;

    // ── LIST ─────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(int page = 1, int? eventId = null)
    {
        if (RequireAuth() is { } r) return r;
        var result = await _api.GetShowtimesAsync(page, eventId);
        ViewBag.EventId = eventId;
        return View(result);
    }

    // ── DETAIL + SEATS ───────────────────────────────────────────────────────
    public async Task<IActionResult> Detail(int id)
    {
        if (RequireAuth() is { } r) return r;
        var showtime = await _api.GetShowtimeAsync(id);
        if (showtime == null) { TempData["Error"] = "Función no encontrada."; return RedirectToAction(nameof(Index)); }
        var seats = await _api.GetShowtimeSeatsAsync(id) ?? new();
        ViewBag.Seats = seats;
        return View(showtime);
    }

    // ── CREATE ───────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (RequireAuth() is { } r) return r;
        var events = await _api.GetEventsAsync(1, true);
        return View(new ShowtimeFormViewModel { Events = events?.Items ?? new() });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateShowtimeRequest request, string seatLayout)
    {
        if (RequireAuth() is { } r) return r;

        if (!string.IsNullOrEmpty(seatLayout))
        {
            try
            {
                var rows = System.Text.Json.JsonSerializer.Deserialize<List<SeatRowRequest>>(
                    seatLayout,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (rows != null) request.SeatLayout = rows;
            }
            catch { /* ignore */ }
        }

        var result = await _api.CreateShowtimeAsync(request);
        if (result == null)
        {
            ViewBag.Error = "No se pudo crear la función. Verifica los datos.";
            var events = await _api.GetEventsAsync(1, true);
            return View(new ShowtimeFormViewModel { Request = request, Events = events?.Items ?? new() });
        }
        TempData["Success"] = "Función creada exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── EDIT ─────────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (RequireAuth() is { } r) return r;
        var showtime = await _api.GetShowtimeAsync(id);
        if (showtime == null) { TempData["Error"] = "Función no encontrada."; return RedirectToAction(nameof(Index)); }
        return View(new ShowtimeEditViewModel
        {
            Showtime = showtime,
            Request  = new UpdateShowtimeRequest { StartTime = showtime.StartTime, BasePrice = showtime.BasePrice }
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, UpdateShowtimeRequest request)
    {
        if (RequireAuth() is { } r) return r;
        var result = await _api.UpdateShowtimeAsync(id, request);
        if (result == null)
        {
            TempData["Error"] = "No se pudo actualizar la función.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        TempData["Success"] = "Función actualizada correctamente.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    // ── DELETE ────────────────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (RequireAuth() is { } r) return r;
        var ok = await _api.DeleteShowtimeAsync(id);
        TempData[ok ? "Success" : "Error"] = ok
            ? "Función eliminada correctamente."
            : "No se pudo eliminar. Puede tener boletas vendidas.";
        return RedirectToAction(nameof(Index));
    }

    // ── TOGGLE STATUS ─────────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> SetStatus(int id, bool active)
    {
        if (RequireAuth() is { } r) return r;
        var result = await _api.SetShowtimeStatusAsync(id, active);
        TempData[result != null ? "Success" : "Error"] = result != null
            ? (active ? "Función activada." : "Función cancelada.")
            : "No se pudo cambiar el estado.";
        return RedirectToAction(nameof(Detail), new { id });
    }
}
