using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers
{
    public class EventsController : Controller
    {
        private readonly RaceDayApiClient _api;

        public EventsController(RaceDayApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(
            string? search, string? type)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToAction("Login", "Account");

            using var response =
                await _api.GetJsonAsync("api/events");

            if (response == null)
            {
                TempData["Error"] =
                    "Unable to load events from the API.";

                return View(JsonDocument.Parse("[]"));
            }

            var events = new List<JsonElement>();

            foreach (var item in response.RootElement
                .EnumerateArray())
            {
                string name = item.GetProperty("name")
                    .GetString() ?? "";

                string eventType =
                    item.GetProperty("eventType")
                        .GetString() ?? "";

                DateTime date = item.GetProperty("eventDate")
                    .GetDateTime();

                if (date.Date < DateTime.Today)
                    continue;

                if (!string.IsNullOrWhiteSpace(search) &&
                    !name.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.IsNullOrWhiteSpace(type) &&
                    !eventType.Contains(
                        type,
                        StringComparison.OrdinalIgnoreCase))
                    continue;

                events.Add(item.Clone());
            }

            ViewBag.Search = search;
            ViewBag.Type = type;

            return View(JsonDocument.Parse(
                JsonSerializer.Serialize(events)));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToAction("Login", "Account");

            var eventDetails =
                await _api.GetJsonAsync($"api/events/{id}");

            if (eventDetails == null)
                return NotFound();

            using var categories = await _api.GetJsonAsync(
                $"api/events/{id}/categories");

            ViewBag.Categories =
                categories?.RootElement.Clone()
                ?? JsonDocument.Parse("[]").RootElement.Clone();

            return View(eventDetails);
        }
    }
}