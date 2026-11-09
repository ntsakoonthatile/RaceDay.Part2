using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = "";

        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; } =
            DateTime.Today.AddDays(7);

        [Required]
        public string Location { get; set; } = "";

        [Range(typeof(decimal), "0.1", "9999")]
        [Display(Name = "Distance (km)")]
        public decimal Distance { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Select an event type.")]
        public int EventTypeId { get; set; }

        public string? BannerImageUrl { get; set; }

        public IFormFile? BannerImage { get; set; }
    }
}