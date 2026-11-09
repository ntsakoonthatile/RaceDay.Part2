using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Helpers;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = SessionAuth.GetUserId(HttpContext);

            if (!userId.HasValue)
            {
                return Unauthorized("Please log in first.");
            }

            var user = await _context.Users.FindAsync(userId.Value);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.Phone,
                user.DateOfBirth,
                user.ProfilePictureUrl
            });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
        {
            var userId = SessionAuth.GetUserId(HttpContext);

            if (!userId.HasValue)
            {
                return Unauthorized("Please log in first.");
            }

            var user = await _context.Users.FindAsync(userId.Value);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Phone = request.Phone;
            user.DateOfBirth = request.DateOfBirth;
            user.ProfilePictureUrl = request.ProfilePictureUrl;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully."
            });
        }
    }
}
