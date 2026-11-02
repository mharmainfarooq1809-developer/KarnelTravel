using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models
{
    public class ReviewSubmissionViewModel
    {
        [Required]
        public string ListingType { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int ListingId { get; set; }

        public string ListingName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your name.")]
        [StringLength(100)]
        [Display(Name = "Your name")]
        public string ReviewerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a review title.")]
        [StringLength(150)]
        [Display(Name = "Review title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please write your experience.")]
        [StringLength(2000, MinimumLength = 10,
            ErrorMessage = "Your review must be between 10 and 2000 characters.")]
        [Display(Name = "Your experience")]
        public string Comment { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Please select a rating.")]
        public int Rating { get; set; }
    }
}