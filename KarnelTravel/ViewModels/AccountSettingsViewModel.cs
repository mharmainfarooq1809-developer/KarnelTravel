using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.ViewModels;

public class AccountSettingsViewModel
{
    [Required, StringLength(128)] public string DisplayName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    [DataType(DataType.Password)] public string CurrentPassword { get; set; } = string.Empty;
    [DataType(DataType.Password), MinLength(8)] public string NewPassword { get; set; } = string.Empty;
    [DataType(DataType.Password), Compare(nameof(NewPassword))] public string ConfirmPassword { get; set; } = string.Empty;
}
