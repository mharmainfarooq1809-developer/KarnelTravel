using KarnelTravel.Models;

namespace KarnelTravel.ViewModels;

public class DemoPaymentCheckoutViewModel
{
    public Booking Booking { get; set; } = null!;
    public DemoPaymentTransaction? LatestTransaction { get; set; }
    public string Currency { get; set; } = "PKR";
    public DemoPaymentFormViewModel Form { get; set; } = new();
}

public class DemoPaymentFormViewModel
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 2)]
    [System.ComponentModel.DataAnnotations.Display(Name = "Cardholder name")]
    public string CardholderName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(23)]
    [System.ComponentModel.DataAnnotations.Display(Name = "Card number")]
    public string CardNumber { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.RegularExpression(@"^(0[1-9]|1[0-2])/\d{2}$", ErrorMessage = "Use MM/YY format.")]
    [System.ComponentModel.DataAnnotations.Display(Name = "Expiry date")]
    public string ExpiryDate { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.RegularExpression(@"^\d{3,4}$", ErrorMessage = "Enter 3 or 4 digits.")]
    public string Cvv { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.StringLength(20)]
    [System.ComponentModel.DataAnnotations.Display(Name = "Billing ZIP / postal code")]
    public string? PostalCode { get; set; }
}

public class DemoPaymentConfirmationViewModel
{
    public Booking Booking { get; set; } = null!;
    public DemoPaymentTransaction Transaction { get; set; } = null!;
    public bool IsSuccessful => Transaction.PaymentStatus is "Successful" or "Paid (Demo)";
}
