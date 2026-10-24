using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models; // <-- Changed from KarnelTravel

public class RegisterViewModel
{
    [Required] public string DisplayName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(6), DataType(DataType.Password)] public string Password { get; set; } = "";
    [DataType(DataType.Password), Compare("Password")] public string ConfirmPassword { get; set; } = "";
}