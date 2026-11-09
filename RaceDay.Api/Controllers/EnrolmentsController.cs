using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Helpers;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost("api/enrolments")]
        public async Task<IActionResult> CreateEnrolment(
            EnrolmentRequest request)
        {
            if (!SessionAuth.HasRole(HttpContext, "Participant"))
            {
                if (!SessionAuth.IsLoggedIn(HttpContext))
                    return Unauthorized("Please log in first.");

                return Forbid();
            }

            var participantId = SessionAuth.GetUserId(HttpContext);

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == request.EventId);

            if (eventItem == null)
            {
                return NotFound("Event not found.");
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == request.CategoryId &&
                    c.EventId == request.EventId);

            if (category == null)
            {
                return BadRequest(
                    "The selected category does not belong to this event.");
            }

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantId == participantId &&
                    e.EventId == request.EventId);

            if (alreadyEnrolled)
            {
                return Conflict(
                    "You are already enrolled in this event.");
            }

            var enrolment = new Enrolment
            {
                ParticipantId = participantId!.Value,
                EventId = request.EventId,
                CategoryId = request.CategoryId,
                EnrolmentDate = DateTime.Now,
                Status = "Confirmed"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Created("", enrolment);
        }

        [HttpGet("api/enrolments/my")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            if (!SessionAuth.HasRole(HttpContext, "Participant"))
            {
                if (!SessionAuth.IsLoggedIn(HttpContext))
                    return Unauthorized("Please log in first.");

                return Forbid();
            }

            var participantId = SessionAuth.GetUserId(HttpContext);

            var enrolments = await _context.Enrolments
                .Where(e => e.ParticipantId == participantId)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Select(e => new
                {
                    e.EnrolmentId,
                    Event = e.Event.Name,
                    e.Event.EventDate,
                    e.Event.Location,
                    Category = e.Category.CategoryName,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        [HttpGet("api/events/{eventId}/enrolments")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            if (!SessionAuth.HasRole(HttpContext, "Organiser"))
            {
                if (!SessionAuth.IsLoggedIn(HttpContext))
                    return Unauthorized("Please log in first.");

                return Forbid();
            }

            var organiserId = SessionAuth.GetUserId(HttpContext);

            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventItem == null)
            {
                return NotFound("Event not found.");
            }

            if (eventItem.OrganiserId != organiserId)
            {
                return Forbid();
            }

            var enrolments = await _context.Enrolments
                .Where(e => e.EventId == eventId)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Select(e => new
                {
                    e.EnrolmentId,
                    Participant = e.Participant.FirstName +
                                  " " +
                                  e.Participant.LastName,
                    e.Participant.Email,
                    Category = e.Category.CategoryName,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        [HttpPut("api/enrolments/{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            StatusRequest request)
        {
            if (!SessionAuth.HasRole(HttpContext, "Organiser"))
            {
                if (!SessionAuth.IsLoggedIn(HttpContext))
                    return Unauthorized("Please log in first.");

                return Forbid();
            }

            if (request.Status != "Pending" &&
                request.Status != "Confirmed" &&
                request.Status != "Cancelled")
            {
                return BadRequest(
                    "Status must be Pending, Confirmed or Cancelled.");
            }

            var organiserId = SessionAuth.GetUserId(HttpContext);

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null)
            {
                return NotFound("Enrolment not found.");
            }

            if (enrolment.Event.OrganiserId != organiserId)
            {
                return Forbid();
            }

            enrolment.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment status updated.",
                enrolment.EnrolmentId,
                enrolment.Status
            });
        }
    }
}
