using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = "";

        [Required]
        [MaxLength(120)]
        public string Email { get; set; } = "";

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = "";

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "";

        [MaxLength(20)]
        public string? Phone { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }

        public ICollection<Event> OrganisedEvents { get; set; }
            = new List<Event>();

        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}