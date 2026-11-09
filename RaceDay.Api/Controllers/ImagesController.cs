
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Helpers;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public ImagesController(
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6_000_000)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            
            if (!SessionAuth.IsLoggedIn(HttpContext))
            {
                return Unauthorized("Please log in first.");
            }


            if (!SessionAuth.HasRole(HttpContext, "Organiser") &&
                !SessionAuth.HasRole(HttpContext, "Participant"))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("Please select an image.");
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(
                    "Image must be smaller than 5 MB.");
            }

            string extension = Path
                .GetExtension(file.FileName)
                .ToLowerInvariant();

            string contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => ""
            };

            if (string.IsNullOrEmpty(contentType) ||
                !string.Equals(
                    file.ContentType,
                    contentType,
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    "Only JPG, PNG and WebP images are allowed.");
            }


            byte[] header = new byte[12];

            try
            {
                using var checkStream = file.OpenReadStream();
                await checkStream.ReadExactlyAsync(header);
            }
            catch (EndOfStreamException)
            {
                return BadRequest("Invalid image file.");
            }

            bool validImage = extension switch
            {
                ".jpg" or ".jpeg" =>
                    header[0] == 0xFF &&
                    header[1] == 0xD8 &&
                    header[2] == 0xFF,

                ".png" =>
                    header[0] == 0x89 &&
                    header[1] == 0x50 &&
                    header[2] == 0x4E &&
                    header[3] == 0x47 &&
                    header[4] == 0x0D &&
                    header[5] == 0x0A &&
                    header[6] == 0x1A &&
                    header[7] == 0x0A,

                ".webp" =>
                    System.Text.Encoding.ASCII.GetString(
                        header, 0, 4) == "RIFF" &&
                    System.Text.Encoding.ASCII.GetString(
                        header, 8, 4) == "WEBP",

                _ => false
            };

            if (!validImage)
            {
                return BadRequest("The image content is invalid.");
            }


            string webRoot = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");

            string uploadsFolder = Path.Combine(
                webRoot, "uploads");

            Directory.CreateDirectory(uploadsFolder);


            string fileName =
                Guid.NewGuid().ToString("N") + extension;

            string filePath =
                Path.Combine(uploadsFolder, fileName);


            using (var stream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write))
            {
                await file.CopyToAsync(stream);
            }

            
            string baseUrl =
                _configuration["Uploads:PublicBaseUrl"]
                ?? $"{Request.Scheme}://{Request.Host}";

            string imageUrl =
                $"{baseUrl.TrimEnd('/')}/uploads/{fileName}";

            return Ok(new
            {
                imageUrl,
                message = "Image uploaded successfully."
            });
        }
    }
}


