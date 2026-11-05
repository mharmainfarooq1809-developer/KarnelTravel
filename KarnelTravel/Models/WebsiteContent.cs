
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace KarnelTravel.Models
{
    public class WebsiteContent
    {
        public int Id { get; set; }

        // ==========================================
        // BRAND IDENTITY
        // ==========================================

        [Required]
        [MaxLength(150)]
        public string SiteName { get; set; } = "Karnel Travel Guide";

        [Required]
        [MaxLength(300)]
        public string Tagline { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? LogoUrl { get; set; } = string.Empty;

        // Temporary logo upload. Not stored in SQL Server.
        [NotMapped]
        public IFormFile? LogoImageFile { get; set; }


        // ==========================================
        // HOMEPAGE
        // ==========================================

        [Required]
        [MaxLength(200)]
        public string HomeHeroHeading { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string HomeHeroSubtitle { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? HomeHeroImageUrl { get; set; } = string.Empty;

        // Temporary homepage hero upload. Not stored in SQL Server.
        [NotMapped]
        public IFormFile? HomeHeroImageFile { get; set; }

        [MaxLength(100)]
        public string? HomeHeroButtonText { get; set; } = "Explore destinations";


        // ==========================================
        // ABOUT PAGE
        // ==========================================

        [Required]
        [MaxLength(200)]
        public string AboutHeading { get; set; } = string.Empty;

        [Required]
        [MaxLength(3000)]
        public string AboutDescription { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Mission { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Vision { get; set; } = string.Empty;


        // ==========================================
        // CONTACT PAGE
        // ==========================================

        [MaxLength(300)]
        public string? ContactAddress { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ContactPhone { get; set; } = string.Empty;

        [MaxLength(256)]
        [EmailAddress]
        public string? ContactEmail { get; set; } = string.Empty;


        // ==========================================
        // WEBSITE FOOTER
        // ==========================================

        [MaxLength(1000)]
        public string? FooterDescription { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? FacebookUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? InstagramUrl { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? YouTubeUrl { get; set; } = string.Empty;


        // ==========================================
        // SEO SETTINGS
        // ==========================================

        [Required]
        [MaxLength(200)]
        public string DefaultMetaTitle { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DefaultMetaDescription { get; set; } = string.Empty;


        // ==========================================
        // RECORD INFORMATION
        // ==========================================

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
