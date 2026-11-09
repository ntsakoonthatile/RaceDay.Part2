using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers
{
    public class EnrolmentController : Controller
    {
        private readonly RaceDayApiClient _api;

        public EnrolmentController(RaceDayApiClient api)
        {
            _api = api;
        }

        private IActionResult? CheckParticipant()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("Role")
                != "Participant")
                return RedirectToAction("Dashboard", "Organiser");

            return null;
        }

        private async Task LoadCategories(int eventId)
        {
            using var result = await _api.GetJsonAsync(
                $"api/events/{eventId}/categories");

            ViewBag.Categories =
                result?.RootElement.Clone()
                ?? JsonDocument.Parse("[]").RootElement.Clone();

            using var eventResult = await _api.GetJsonAsync(
                $"api/events/{eventId}");

            ViewBag.EventName = eventResult == null
                ? "Event"
                : eventResult.RootElement
                    .GetProperty("name").GetString();
        }

        [HttpGet]
        public async Task<IActionResult> Enter(int eventId)
        {
            var access = CheckParticipant();
            if (access != null)
                return access;

            if (eventId <= 0)
                return BadRequest();

            await LoadCategories(eventId);

            return View(new EnrolmentViewModel
            {
                EventId = eventId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enter(
            EnrolmentViewModel model)
        {
            var access = CheckParticipant();
            if (access != null)
                return access;

            if (!ModelState.IsValid)
            {
                await LoadCategories(model.EventId);
                return View(model);
            }

            var request = new
            {
                EventId = model.EventId,
                CategoryId = model.CategoryId
            };

            bool success = await _api.PostJsonAsync(
                "api/enrolments", request);

            if (success)
            {
                TempData["Success"] =
                    "You have enrolled successfully!";

                return RedirectToAction(
                    "MyEvents", "Participant");
            }

            ModelState.AddModelError(
                "",
                "Unable to enrol. You may already be " +
                "entered in this event.");

            await LoadCategories(model.EventId);
            return View(model);
        }
    }
}
