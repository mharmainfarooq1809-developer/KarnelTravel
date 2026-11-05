using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class ApplicationSettings
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string ApplicationName { get; set; } = "Karnel Travel Guide";
    [Required, StringLength(100)]
    public string OrganizationName { get; set; } = "Karnel Travel Guide";
    [Required, StringLength(10)]
    public string DefaultLanguage { get; set; } = "en-US";
    [Required, StringLength(80)]
    public string DefaultCountry { get; set; } = "Pakistan";
    [Required, StringLength(10)]
    public string Currency { get; set; } = "PKR";
    [Required, StringLength(80)]
    public string TimeZone { get; set; } = "Pakistan Standard Time";
    [Required, StringLength(30)]
    public string DateFormat { get; set; } = "dd MMM yyyy";
    [Required, StringLength(30)]
    public string TimeFormat { get; set; } = "hh:mm tt";
    [StringLength(10)]
    public string CountryCallingCode { get; set; } = "+92";
    [EmailAddress, StringLength(256)]
    public string ContactEmail { get; set; } = "";
    [Phone, StringLength(30)]
    public string ContactPhone { get; set; } = "";

    public bool MaintenanceMode { get; set; }
    public bool PublicWebsiteVisible { get; set; } = true;
    public bool AllowBookingInquiries { get; set; } = true;
    public bool AllowContactSubmissions { get; set; } = true;
    public bool AllowFeedbackSubmissions { get; set; } = true;
    public bool ShowDemoContentNotice { get; set; }
    [StringLength(180)] public string DefaultSeoTitle { get; set; } = "Karnel Travel Guide";
    [StringLength(320)] public string DefaultSeoDescription { get; set; } = "Discover memorable places and experiences in Pakistan.";

    public bool BookingEnabled { get; set; } = true;
    [Required, StringLength(30)] public string DefaultBookingStatus { get; set; } = "Pending";
    public bool AllowGuestBookings { get; set; } = true;
    public bool RequirePhone { get; set; } = true;
    public bool RequireEmail { get; set; }
    [Range(0, 365)] public int MinimumAdvanceBookingDays { get; set; } = 0;
    [Range(1, 730)] public int MaximumAdvanceBookingDays { get; set; } = 365;
    [Range(1, 100)] public int DefaultAdults { get; set; } = 1;
    [Range(1, 100)] public int MaximumAdults { get; set; } = 100;
    [Range(0, 100)] public int MaximumChildren { get; set; } = 100;
    [StringLength(10)] public string BookingCurrency { get; set; } = "PKR";
    [StringLength(20)] public string BookingReferencePrefix { get; set; } = "KTL";
    [StringLength(30)] public string BookingConfirmationBehavior { get; set; } = "Inquiry";

    public bool AdminNotificationIndicators { get; set; } = true;
    public bool NotifyNewBooking { get; set; } = true;
    public bool NotifyNewContact { get; set; } = true;
    public bool NotifyNewFeedback { get; set; } = true;
    public bool NotifyBookingStatusChange { get; set; } = true;
    [EmailAddress, StringLength(256)] public string NotificationEmail { get; set; } = "";
    [StringLength(100)] public string NotificationSubjectPrefix { get; set; } = "[Karnel Travel]";

    [Required, StringLength(20)] public string AdminTheme { get; set; } = "System";
    [Required, StringLength(20)] public string SidebarBehavior { get; set; } = "Expanded";
    [Required, StringLength(20)] public string InterfaceDensity { get; set; } = "Comfortable";
    [Required, StringLength(20)] public string DashboardDateRange { get; set; } = "30 days";
    [Range(10, 100)] public int TablePageSize { get; set; } = 25;
    [Required, StringLength(20)] public string DashboardLayout { get; set; } = "Editorial";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [StringLength(256)] public string? UpdatedBy { get; set; }
}
