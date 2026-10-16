using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Helpers;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            if (!SessionAuth.IsLoggedIn(HttpContext))
            {
                return Unauthorized("Please log in first.");
            }

            var events = await _context.Events
                .Include(e => e.EventType)
                .Include(e => e.Organiser)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.DistanceKm,
                    e.BannerImageUrl,
                    EventType = e.EventType.TypeName,
                    Organiser = e.Organiser.FirstName + " " + e.Organiser.LastName
                })
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            if (!SessionAuth.IsLoggedIn(HttpContext))
            {
                return Unauthorized("Please log in first.");
            }

            var eventItem = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
            {
                return NotFound("Event not found.");
            }

            return Ok(eventItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventRequest request)
        {
            if (!SessionAuth.HasRole(HttpContext, "Organiser"))
                return StatusCode(StatusCodes.Status403Forbidden);

            {
                if (!SessionAuth.IsLoggedIn(HttpContext))
                    return Unauthorized("Please log in first.");

                return Forbid();
            }

            var userId = SessionAuth.GetUserId(HttpContext);

            if (!await _context.EventTypes.AnyAsync(x =>
                x.EventTypeId == request.EventTypeId))
            {
                return BadRequest("Invalid event type.");
            }

            if (request.DistanceKm <= 0)
            {
                return BadRequest("Distance must be greater than zero.");
            }

            var newEvent = new Event
            {
                OrganiserId = userId!.Value,
                EventTypeId = request.EventTypeId,
                Name = request.Name,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                DistanceKm = request.DistanceKm,
                BannerImageUrl = request.BannerImageUrl
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = newEvent.EventId },
                newEvent);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            EventRequest request)
        {
            if (!SessionAuth.HasRole(HttpContext, "Organiser"))
            {
                if (!SessionAuth.IsLoggedIn(HttpContext))
                    return Unauthorized("Please log in first.");

                return Forbid();
            }

            var userId = SessionAuth.GetUserId(HttpContext);

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
            {
                return NotFound("Event not found.");
            }

            if (eventItem.OrganiserId != userId)
            {
                return Forbid();
            }

            if (!await _context.EventTypes.AnyAsync(x =>
                x.EventTypeId == request.EventTypeId))
            {
                return BadRequest("Invalid event type.");
            }

            eventItem.EventTypeId = request.EventTypeId;
            eventItem.Name = request.Name;
            eventItem.Description = request.Description;
            eventItem.EventDate = request.EventDate;
            eventItem.Location = request.Location;
            eventItem.DistanceKm = request.DistanceKm;
            eventItem.BannerImageUrl = request.BannerImageUrl;

            await _context.SaveChangesAsync();

            return Ok(eventItem);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!SessionAuth.HasRole(HttpContext, "Organiser"))
            {
                if (!SessionAuth.IsLoggedIn(HttpContext))
                    return Unauthorized("Please log in first.");

                return Forbid();
            }

            var userId = SessionAuth.GetUserId(HttpContext);

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
            {
                return NotFound("Event not found.");
            }

            if (eventItem.OrganiserId != userId)
            {
                return Forbid();
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
