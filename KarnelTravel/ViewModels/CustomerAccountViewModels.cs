using System.ComponentModel.DataAnnotations;

namespace KarnelTravel.ViewModels;

public class CustomerRegisterViewModel
{
    [Required, StringLength(150)] public string FullName { get; set; } = string.Empty;
    [Required, Phone, StringLength(30)] public string PhoneNumber { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required, Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = string.Empty;
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions.")] public bool AcceptTerms { get; set; }
}

public class CustomerLoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = string.Empty;
    [Required, RegularExpression("^[0-9]{6}$", ErrorMessage = "Enter the six-digit code from your email.")] public string Code { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required, Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = string.Empty;
}

public class CustomerProfileViewModel
{
    [Required, StringLength(150)] public string FullName { get; set; } = string.Empty;
    [Required, Phone, StringLength(30)] public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    [DataType(DataType.Password)] public string CurrentPassword { get; set; } = string.Empty;
    [StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string NewPassword { get; set; } = string.Empty;
    [Compare(nameof(NewPassword)), DataType(DataType.Password)] public string ConfirmNewPassword { get; set; } = string.Empty;
}
