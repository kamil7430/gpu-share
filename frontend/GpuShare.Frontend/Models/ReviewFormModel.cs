using System.ComponentModel.DataAnnotations;
using Xunit.Sdk;

namespace GpuShare.Frontend.Models
{
    public class ReviewFormModel
    {
        [Range(0.5, 5.0, ErrorMessage = "Please select a rating.")]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Review text is required.")]
        [MinLength(10, ErrorMessage = "Review must contain at least 10 characters.")]
        [MaxLength(1000, ErrorMessage = "Review is too long.")]
        public string Comment { get; set; } = "";
    }
}
