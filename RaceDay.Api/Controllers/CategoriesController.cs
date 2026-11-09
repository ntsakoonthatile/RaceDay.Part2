using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Helpers;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet("api/events/{eventId}/categories")]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            if (!SessionAuth.IsLoggedIn(HttpContext))
            {
                return Unauthorized("Please log in first.");
            }

            if (!await _context.Events.AnyAsync(e => e.EventId == eventId))
            {
                return NotFound("Event not found.");
            }

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPost("api/events/{eventId}/categories")]
        public async Task<IActionResult> CreateCategory(
            int eventId,
            CategoryRequest request)
        {
            if (!SessionAuth.HasRole(HttpContext, "Organiser"))
            {
                if (!SessionAuth.IsLoggedIn(HttpContext))
                    return Unauthorized("Please log in first.");

                return Forbid();
            }

            var userId = SessionAuth.GetUserId(HttpContext);

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventItem == null)
            {
                return NotFound("Event not found.");
            }

            if (eventItem.OrganiserId != userId)
            {
                return Forbid();
            }

            var duplicate = await _context.Categories
                .AnyAsync(c =>
                    c.EventId == eventId &&
                    c.CategoryName == request.CategoryName);

            if (duplicate)
            {
                return Conflict("This category already exists for the event.");
            }

            var category = new Category
            {
                EventId = eventId,
                CategoryName = request.CategoryName,
                Description = request.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Created("", category);
        }

        [HttpPut("api/categories/{id}")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            CategoryRequest request)
        {
            if (!SessionAuth.HasRole(HttpContext, "Organiser"))
            {
                if (!SessionAuth.IsLoggedIn(HttpContext))
                    return Unauthorized("Please log in first.");

                return Forbid();
            }

            var userId = SessionAuth.GetUserId(HttpContext);

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            if (category.Event.OrganiserId != userId)
            {
                return Forbid();
            }

            category.CategoryName = request.CategoryName;
            category.Description = request.Description;

            await _context.SaveChangesAsync();

            return Ok(category);
        }

        [HttpDelete("api/categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!SessionAuth.HasRole(HttpContext, "Organiser"))
            {
                if (!SessionAuth.IsLoggedIn(HttpContext))
                    return Unauthorized("Please log in first.");

                return Forbid();
            }

            var userId = SessionAuth.GetUserId(HttpContext);

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            if (category.Event.OrganiserId != userId)
            {
                return Forbid();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
