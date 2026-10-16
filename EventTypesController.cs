using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Helpers;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/event-types")]
    public class EventTypesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventTypesController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEventTypes()
        {
            if (!SessionAuth.IsLoggedIn(HttpContext))
            {
                return Unauthorized("Please log in first.");
            }

            var types = await _context.EventTypes
                .OrderBy(x => x.TypeName)
                .ToListAsync();

            return Ok(types);
        }
    }
}
