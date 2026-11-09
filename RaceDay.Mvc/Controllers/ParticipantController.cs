
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiClient _api;

        public ParticipantController(RaceDayApiClient api)
        {
            _api = api;
        }

        private IActionResult? CheckParticipant()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToAction("Login", "Account");

            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Dashboard", "Organiser");

            return null;
        }

        public async Task<IActionResult> MyEvents()
        {
            var access = CheckParticipant();
            if (access != null)
                return access;

            var result =
                await _api.GetJsonAsync("api/enrolments/my");

            return View(result ?? JsonDocument.Parse("[]"));
        }

        public async Task<IActionResult> MyResults()
        {
            var access = CheckParticipant();
            if (access != null)
                return access;

            using var result =
                await _api.GetJsonAsync("api/results/my");

            if (result == null)
                return View(JsonDocument.Parse("[]"));

            var published = new List<JsonElement>();

            foreach (var item in result.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("published", out var value) &&
                    value.ValueKind == JsonValueKind.True)
                {
                    published.Add(item.Clone());
                }
            }

            return View(JsonDocument.Parse(
                JsonSerializer.Serialize(published)));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var access = CheckParticipant();
            if (access != null)
                return access;

            using var result =
                await _api.GetJsonAsync("api/users/me");

            if (result == null)
                return RedirectToAction("Login", "Account");

            var item = result.RootElement;

            var model = new ProfileViewModel
            {
                FirstName =
                    item.GetProperty("firstName").GetString() ?? "",

                LastName =
                    item.GetProperty("lastName").GetString() ?? "",

                Email =
                    item.GetProperty("email").GetString() ?? "",

                Phone =
                    item.GetProperty("phone").GetString(),

                ProfilePictureUrl =
                    item.GetProperty("profilePictureUrl").GetString()
            };

            if (item.TryGetProperty("dateOfBirth", out var dob) &&
                dob.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(dob.GetString(), out var date))
            {
                model.DateOfBirth = date;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(
            ProfileViewModel model,
            IFormFile? profilePicture)
        {
            var access = CheckParticipant();
            if (access != null)
                return access;

            if (!ModelState.IsValid)
                return View(model);

            using var current =
                await _api.GetJsonAsync("api/users/me");

            if (current == null)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to load your current profile.");

                return View(model);
            }

            string? pictureUrl = current.RootElement
                .GetProperty("profilePictureUrl").GetString();

            if (profilePicture != null &&
                profilePicture.Length > 0)
            {
                if (profilePicture.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "",
                        "Profile picture must be 5 MB or smaller.");

                    return View(model);
                }

                pictureUrl =
                    await _api.UploadImageAsync(profilePicture);

                if (string.IsNullOrWhiteSpace(pictureUrl))
                {
                    ModelState.AddModelError(
                        "",
                        "Profile picture upload failed. " +
                        "Check the Azure storage setup.");

                    return View(model);
                }
            }

            var request = new
            {
                model.FirstName,
                model.LastName,
                model.Phone,
                model.DateOfBirth,
                ProfilePictureUrl = pictureUrl
            };

            bool success =
                await _api.PutJsonAsync("api/users/me", request);

            if (success)
            {
                HttpContext.Session.SetString(
                    "FirstName", model.FirstName);

                TempData["Success"] =
                    "Profile updated successfully.";

                return RedirectToAction(nameof(Profile));
            }

            ModelState.AddModelError(
                "", "Unable to update your profile.");

            return View(model);
        }
    }
}
