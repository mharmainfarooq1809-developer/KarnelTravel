using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.Models;

public class RegisterViewModel
{
    [Required, StringLength(150)] public string DisplayName { get; set; } = "";
    [Required, Phone, StringLength(30)] public string PhoneNumber { get; set; } = "";
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, DataType(DataType.Password), Compare("Password")] public string ConfirmPassword { get; set; } = "";
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions.")] public bool AcceptTerms { get; set; }
}