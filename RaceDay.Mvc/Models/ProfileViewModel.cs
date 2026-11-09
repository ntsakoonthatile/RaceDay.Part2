using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class ProfileViewModel
    {
        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        public string Email { get; set; } = "";

        public string? Phone { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string? ProfilePictureUrl { get; set; }
    }
}