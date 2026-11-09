
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiClient _api;

        public OrganiserController(RaceDayApiClient api)
        {
            _api = api;
        }

        private IActionResult? CheckAccess()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (HttpContext.Session.GetString("Role") != "Organiser")
            {
                return RedirectToAction("Index", "Events");
            }

            return null;
        }

        private static string ReadString(JsonElement item, string property)
        {
            if (item.TryGetProperty(property, out var value) &&
                value.ValueKind != JsonValueKind.Null)
            {
                return value.ToString();
            }

            return "";
        }

        private static int ReadInt(JsonElement item, string property)
        {
            if (item.TryGetProperty(property, out var value) &&
                value.TryGetInt32(out int number))
            {
                return number;
            }

            return 0;
        }

        private static decimal ReadDecimal(JsonElement item, string property)
        {
            if (item.TryGetProperty(property, out var value) &&
                value.TryGetDecimal(out decimal number))
            {
                return number;
            }

            return 0;
        }

        private async Task<JsonDocument> GetMyEventsAsync()
        {
            using var allEvents = await _api.GetJsonAsync("api/events");

            var myEvents = new List<object>();
            int organiserId =
                HttpContext.Session.GetInt32("UserId") ?? 0;

            if (allEvents == null ||
                allEvents.RootElement.ValueKind != JsonValueKind.Array)
            {
                return JsonDocument.Parse("[]");
            }

            foreach (var item in allEvents.RootElement.EnumerateArray())
            {
                int eventId = ReadInt(item, "eventId");

                if (eventId <= 0)
                    continue;

                using var details =
                    await _api.GetJsonAsync($"api/events/{eventId}");

                if (details == null ||
                    ReadInt(details.RootElement, "organiserId")
                        != organiserId)
                {
                    continue;
                }

                myEvents.Add(new
                {
                    eventId,
                    name = ReadString(item, "name"),
                    eventName = ReadString(item, "name"),
                    description = ReadString(item, "description"),
                    eventDate = ReadString(item, "eventDate"),
                    location = ReadString(item, "location"),
                    distanceKm = ReadDecimal(item, "distanceKm"),
                    distance = ReadDecimal(item, "distanceKm"),
                    eventType = ReadString(item, "eventType"),
                    bannerImageUrl = ReadString(item, "bannerImageUrl")
                });
            }

            return JsonDocument.Parse(JsonSerializer.Serialize(myEvents));
        }

        private async Task LoadEventTypesAsync()
        {
            var types = await _api.GetJsonAsync("api/event-types");

            ViewBag.EventTypes = types?.RootElement.Clone();

            types?.Dispose();
        }

        // ======================================
        // ORGANISER DASHBOARD
        // ======================================

        public async Task<IActionResult> Dashboard()
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            var events = await GetMyEventsAsync();

            int totalEvents = 0;
            int upcomingEvents = 0;
            int totalEnrolments = 0;

            foreach (var item in events.RootElement.EnumerateArray())
            {
                totalEvents++;

                if (DateTime.TryParse(
                    ReadString(item, "eventDate"),
                    out DateTime eventDate))
                {
                    if (eventDate >= DateTime.Today)
                        upcomingEvents++;
                }

                int eventId = ReadInt(item, "eventId");

                using var enrolments = await _api.GetJsonAsync(
                    $"api/events/{eventId}/enrolments");

                if (enrolments != null &&
                    enrolments.RootElement.ValueKind ==
                        JsonValueKind.Array)
                {
                    totalEnrolments +=
                        enrolments.RootElement.GetArrayLength();
                }
            }

            ViewBag.TotalEvents = totalEvents;
            ViewBag.UpcomingEvents = upcomingEvents;
            ViewBag.TotalEnrolments = totalEnrolments;

            return View(events);
        }

        public async Task<IActionResult> Events()
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            var events = await GetMyEventsAsync();

            return View(events);
        }

        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            await LoadEventTypesAsync();

            return View(new EventViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventViewModel model)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            if (!ModelState.IsValid)
            {
                await LoadEventTypesAsync();
                return View(model);
            }

            string? bannerUrl = model.BannerImageUrl;

            if (model.BannerImage != null)
            {
                bannerUrl =
                    await _api.UploadImageAsync(model.BannerImage);

                if (string.IsNullOrWhiteSpace(bannerUrl))
                {
                    ModelState.AddModelError(
                        "",
                        "The banner could not be uploaded.");

                    await LoadEventTypesAsync();
                    return View(model);
                }
            }

            // These names match Part 2 EventRequest.cs.
            var request = new
            {
                Name = model.EventName,
                Description = model.Description,
                EventDate = model.EventDate,
                Location = model.Location,
                DistanceKm = model.Distance,
                EventTypeId = model.EventTypeId,
                BannerImageUrl = bannerUrl
            };

            bool success =
                await _api.PostJsonAsync("api/events", request);

            if (success)
            {
                TempData["Success"] =
                    "Event created successfully.";

                return RedirectToAction(nameof(Events));
            }

            ModelState.AddModelError(
                "",
                "Unable to create the event. Check the details.");

            await LoadEventTypesAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            using var result =
                await _api.GetJsonAsync($"api/events/{id}");

            if (result == null)
                return NotFound();

            var item = result.RootElement;

            int organiserId =
                HttpContext.Session.GetInt32("UserId") ?? 0;

            if (ReadInt(item, "organiserId") != organiserId)
            {
                return Forbid();
            }

            DateTime.TryParse(
                ReadString(item, "eventDate"),
                out DateTime eventDate);

            var model = new EventViewModel
            {
                EventId = ReadInt(item, "eventId"),
                EventName = ReadString(item, "name"),
                Description = ReadString(item, "description"),
                EventDate = eventDate,
                Location = ReadString(item, "location"),
                Distance = ReadDecimal(item, "distanceKm"),
                EventTypeId = ReadInt(item, "eventTypeId"),
                BannerImageUrl = ReadString(item, "bannerImageUrl")
            };

            await LoadEventTypesAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(EventViewModel model)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            if (!ModelState.IsValid)
            {
                await LoadEventTypesAsync();
                return View(model);
            }

            string? bannerUrl = model.BannerImageUrl;

            if (model.BannerImage != null)
            {
                bannerUrl =
                    await _api.UploadImageAsync(model.BannerImage);

                if (string.IsNullOrWhiteSpace(bannerUrl))
                {
                    ModelState.AddModelError(
                        "",
                        "The banner could not be uploaded.");

                    await LoadEventTypesAsync();
                    return View(model);
                }
            }

            var request = new
            {
                Name = model.EventName,
                Description = model.Description,
                EventDate = model.EventDate,
                Location = model.Location,
                DistanceKm = model.Distance,
                EventTypeId = model.EventTypeId,
                BannerImageUrl = bannerUrl
            };

            bool success = await _api.PutJsonAsync(
                $"api/events/{model.EventId}",
                request);

            if (success)
            {
                TempData["Success"] =
                    "Event updated successfully.";

                return RedirectToAction(nameof(Events));
            }

            ModelState.AddModelError(
                "",
                "Unable to update the event.");

            await LoadEventTypesAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            bool success =
                await _api.DeleteAsync($"api/events/{id}");

            TempData[success ? "Success" : "Error"] =
                success
                    ? "Event deleted successfully."
                    : "Unable to delete this event.";

            return RedirectToAction(nameof(Events));
        }

        public async Task<IActionResult> Categories(int eventId)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            if (eventId <= 0)
                return RedirectToAction(nameof(Events));

            ViewBag.EventId = eventId;

            var categories = await _api.GetJsonAsync(
                $"api/events/{eventId}/categories");

            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory(int eventId)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            if (eventId <= 0)
                return RedirectToAction(nameof(Events));

            return View(new CategoryViewModel
            {
                EventId = eventId
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(
            CategoryViewModel model)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            if (string.IsNullOrWhiteSpace(model.CategoryName))
            {
                ModelState.AddModelError(
                    nameof(model.CategoryName),
                    "Category name is required.");
            }

            if (!ModelState.IsValid)
                return View(model);

            string? description = model.Description;

            if (string.IsNullOrWhiteSpace(description) &&
                (model.MinimumAge > 0 ||
                 model.MaximumAge > 0 ||
                 model.Distance > 0))
            {
                description =
                    $"Age: {model.MinimumAge}-{model.MaximumAge}; " +
                    $"Distance: {model.Distance} km";
            }

            var request = new
            {
                CategoryName = model.CategoryName,
                Description = description
            };

            bool success = await _api.PostJsonAsync(
                $"api/events/{model.EventId}/categories",
                request);

            if (success)
            {
                TempData["Success"] =
                    "Category created successfully.";

                return RedirectToAction(
                    nameof(Categories),
                    new { eventId = model.EventId });
            }

            ModelState.AddModelError(
                "",
                "Unable to create the category.");

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(
            int id, int eventId)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            using var categories = await _api.GetJsonAsync(
                $"api/events/{eventId}/categories");

            if (categories == null ||
                categories.RootElement.ValueKind != JsonValueKind.Array)
            {
                return NotFound();
            }

            foreach (var item in categories.RootElement.EnumerateArray())
            {
                if (ReadInt(item, "categoryId") == id)
                {
                    var model = new CategoryViewModel
                    {
                        CategoryId = id,
                        EventId = eventId,
                        CategoryName = ReadString(item, "categoryName"),
                        Description = ReadString(item, "description")
                    };

                    return View(model);
                }
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(
            CategoryViewModel model)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            if (!ModelState.IsValid)
                return View(model);

            var request = new
            {
                CategoryName = model.CategoryName,
                Description = model.Description
            };

            bool success = await _api.PutJsonAsync(
                $"api/categories/{model.CategoryId}",
                request);

            if (success)
            {
                TempData["Success"] =
                    "Category updated successfully.";

                return RedirectToAction(
                    nameof(Categories),
                    new { eventId = model.EventId });
            }

            ModelState.AddModelError(
                "",
                "Unable to update the category.");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(
            int id, int eventId)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            bool success =
                await _api.DeleteAsync($"api/categories/{id}");

            TempData[success ? "Success" : "Error"] =
                success
                    ? "Category deleted successfully."
                    : "Unable to delete the category.";

            return RedirectToAction(
                nameof(Categories),
                new { eventId });
        }


        public async Task<IActionResult> Enrolments(int eventId)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            if (eventId <= 0)
                return RedirectToAction(nameof(Events));

            ViewBag.EventId = eventId;

            using var result = await _api.GetJsonAsync(
                $"api/events/{eventId}/enrolments");

            if (result == null ||
                result.RootElement.ValueKind != JsonValueKind.Array)
            {
                return View(JsonDocument.Parse("[]"));
            }

            var enrolments = new List<object>();

            foreach (var item in result.RootElement.EnumerateArray())
            {
                enrolments.Add(new
                {
                    enrolmentId = ReadInt(item, "enrolmentId"),
                    participant = ReadString(item, "participant"),
                    participantName = ReadString(item, "participant"),
                    category = ReadString(item, "category"),
                    categoryName = ReadString(item, "category"),
                    email = ReadString(item, "email"),
                    status = ReadString(item, "status"),
                    enrolmentDate = ReadString(item, "enrolmentDate")
                });
            }

            return View(JsonDocument.Parse(
                JsonSerializer.Serialize(enrolments)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEnrolmentStatus(
            int id, int eventId, string status)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            if (status != "Pending" &&
                status != "Confirmed" &&
                status != "Cancelled")
            {
                TempData["Error"] = "Invalid enrolment status.";

                return RedirectToAction(
                    nameof(Enrolments),
                    new { eventId });
            }

            bool success = await _api.PutJsonAsync(
                $"api/enrolments/{id}/status",
                new { Status = status });

            TempData[success ? "Success" : "Error"] =
                success
                    ? "Enrolment status updated."
                    : "Unable to update enrolment status.";

            return RedirectToAction(
                nameof(Enrolments),
                new { eventId });
        }

        // ======================================
        // EVENT RESULTS
        // ======================================

        [HttpGet]
        public async Task<IActionResult> Results(int eventId)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            if (eventId <= 0)
                return RedirectToAction(nameof(Events));

            ViewBag.EventId = eventId;

            // Get the enrolments that can receive results.
            var enrolments = await _api.GetJsonAsync(
                $"api/events/{eventId}/enrolments");

            return View(enrolments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(
            int enrolmentId,
            string finishTime,
            int position,
            int finishPosition = 0,
            int eventId = 0,
            bool published = true)
        {
            var access = CheckAccess();
            if (access != null)
                return access;

            int actualPosition =
                finishPosition > 0 ? finishPosition : position;

            if (!TimeSpan.TryParse(finishTime, out var parsedTime) ||
                parsedTime <= TimeSpan.Zero ||
                actualPosition <= 0)
            {
                TempData["Error"] =
                    "Enter a valid finish time and position.";

                return RedirectToAction(
                    eventId > 0 ? nameof(Results) : nameof(Events),
                    eventId > 0 ? new { eventId } : null);
            }

            var request = new
            {
                EnrolmentId = enrolmentId,
                FinishTime = parsedTime,
                FinishPosition = actualPosition,
                Published = published
            };

            bool success =
                await _api.PostJsonAsync("api/results", request);

            TempData[success ? "Success" : "Error"] =
                success
                    ? "Result saved successfully."
                    : "Unable to save the result. It may already exist.";

            return RedirectToAction(
                eventId > 0 ? nameof(Results) : nameof(Events),
                eventId > 0 ? new { eventId } : null);
        }
    }
}
