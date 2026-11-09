using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class EnrolmentViewModel
    {
        [Range(1, int.MaxValue)]
        public int EventId { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
    }
}

