using System;
using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models
{
    public class ContactInquiry
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Phone, StringLength(30)]
        public string? Phone { get; set; }

        [Required, StringLength(50)]
        public string Category { get; set; } = "General";

        [Required, StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Admin-managed fields
        [Required, StringLength(30)]
        public string Status { get; set; } = "New";

        [StringLength(2000)]
        public string? AdminNotes { get; set; }
    }
}