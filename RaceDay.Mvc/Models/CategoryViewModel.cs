
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = "";

        [Display(Name = "Description")]
        public string? Description { get; set; }

        public int MinimumAge { get; set; }

        public int MaximumAge { get; set; }

        public decimal Distance { get; set; }
    }
}



