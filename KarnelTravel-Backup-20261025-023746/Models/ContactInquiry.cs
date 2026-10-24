using System;
using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models
{
    public class ContactInquiry
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        [Required]
        public string Category { get; set; }

        [Required, StringLength(2000)]
        public string Message { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
