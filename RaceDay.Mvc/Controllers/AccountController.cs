using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly RaceDayApiClient _api;

        public AccountController(RaceDayApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using var result = await _api.LoginAsync(
                    model.Email, model.Password);

                if (result == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid email or password.");

                    return View(model);
                }

                JsonElement user = result.RootElement;

                int userId = user.GetProperty("userId")
                    .GetInt32();

                string role = user.GetProperty("role")
                    .GetString() ?? "";

                string firstName = user.GetProperty("firstName")
                    .GetString() ?? "";

                HttpContext.Session.SetInt32("UserId", userId);
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetString(
                    "FirstName", firstName);

                if (role == "Organiser")
                {
                    return RedirectToAction(
                        "Dashboard", "Organiser");
                }

                if (role == "Participant")
                {
                    return RedirectToAction(
                        "Index", "Events");
                }

                HttpContext.Session.Clear();

                ModelState.AddModelError(
                    "", "Unknown account role.");

                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "Cannot connect to the RaceDay API. " +
                    "Make sure RaceDay.Api is running.");

                return View(model);
            }
            catch (JsonException)
            {
                ModelState.AddModelError(
                    "", "Unexpected login response.");

                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var request = new
            {
                model.FirstName,
                model.LastName,
                model.Email,
                model.Password,
                model.Role,
                model.Phone,
                model.DateOfBirth
            };

            try
            {
                bool success = await _api.RegisterAsync(request);

                if (success)
                {
                    TempData["Success"] =
                        "Registration successful. Please log in.";

                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError(
                    "",
                    "Registration failed. Check your details " +
                    "or try another email address.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "", "Cannot connect to the RaceDay API.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _api.LogoutAsync();
            }
            finally
            {
                HttpContext.Session.Clear();
            }

            return RedirectToAction(nameof(Login));
        }
    }
}