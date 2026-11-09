using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Helpers;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost("api/results")]
        public async Task<IActionResult> CreateResult(
            ResultRequest request)
        {
            if (!SessionAuth.HasRole(HttpContext, "Organiser"))
            {
                if (!SessionAuth.IsLoggedIn(HttpContext))
                    return Unauthorized("Please log in first.");

                return Forbid();
            }

            var organiserId = SessionAuth.GetUserId(HttpContext);

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == request.EnrolmentId);

            if (enrolment == null)
            {
                return NotFound("Enrolment not found.");
            }

            if (enrolment.Event.OrganiserId != organiserId)
            {
                return Forbid();
            }

            if (request.FinishPosition <= 0)
            {
                return BadRequest(
                    "Finish position must be greater than zero.");
            }

            var existingResult = await _context.Results
                .AnyAsync(r =>
                    r.EnrolmentId == request.EnrolmentId);

            if (existingResult)
            {
                return Conflict(
                    "A result already exists for this enrolment.");
            }

            var result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                FinishTime = request.FinishTime,
                FinishPosition = request.FinishPosition,
                Published = request.Published
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Created("", result);
        }

        [HttpGet("api/results/my")]
        public async Task<IActionResult> GetMyResults()
        {
            if (!SessionAuth.HasRole(HttpContext, "Participant"))
            {
                if (!SessionAuth.IsLoggedIn(HttpContext))
                    return Unauthorized("Please log in first.");

                return Forbid();
            }

            var participantId = SessionAuth.GetUserId(HttpContext);

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Category)
                .Where(r =>
                    r.Enrolment.ParticipantId == participantId)
                .Select(r => new
                {
                    r.ResultId,
                    Event = r.Enrolment.Event.Name,
                    Category = r.Enrolment.Category.CategoryName,
                    r.FinishTime,
                    r.FinishPosition,
                    r.Published
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("api/results/{id}")]
        public async Task<IActionResult> GetResult(int id)
        {
            if (!SessionAuth.IsLoggedIn(HttpContext))
            {
                return Unauthorized("Please log in first.");
            }

            var userId = SessionAuth.GetUserId(HttpContext);
            var role = SessionAuth.GetRole(HttpContext);

            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Category)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
            {
                return NotFound("Result not found.");
            }

            if (role == "Participant")
            {
                if (result.Enrolment.ParticipantId != userId)
                {
                    return Forbid();
                }
            }
            else if (role == "Organiser")
            {
                if (result.Enrolment.Event.OrganiserId != userId)
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }

            return Ok(result);
        }
    }
}
