using System.Globalization;
using System.Text.RegularExpressions;

namespace KarnelTravel.Services;

public sealed record DemoCardValidationResult(
    bool IsValid,
    string? Error,
    string? CardBrand,
    string? MaskedCardNumber,
    string Outcome);

public sealed class DemoCardValidationService
{
    private static readonly IReadOnlyDictionary<string, (string Brand, string Outcome, int CvvLength)> TestCards =
        new Dictionary<string, (string, string, int)>(StringComparer.Ordinal)
        {
            ["4242424242424242"] = ("Visa", "Successful", 3),
            ["4374454878787887"] = ("Visa", "Successful", 3),
            ["4000000000000002"] = ("Visa", "Failed", 3),
            ["4000000000009995"] = ("Visa", "InsufficientFunds", 3),
            ["4000000000009987"] = ("Visa", "Cancelled", 3),
            ["5555555555554444"] = ("Mastercard", "Successful", 3)
        };

    public DemoCardValidationResult Validate(string? cardholderName, string? cardNumber, string? expiryDate, string? cvv, string? postalCode, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(cardholderName) || cardholderName.Trim().Length is < 2 or > 100 ||
            !Regex.IsMatch(cardholderName.Trim(), @"^[\p{L}][\p{L}\s'’-]*$", RegexOptions.CultureInvariant))
            return Invalid("Enter a valid cardholder name.");

        var normalizedNumber = new string((cardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        if (normalizedNumber.Length is < 13 or > 19 || !IsLuhnValid(normalizedNumber) || !TestCards.TryGetValue(normalizedNumber, out var card))
            return Invalid("Use one of the configured demo card numbers shown below.");

        if (!DateTime.TryParseExact(expiryDate?.Trim(), "MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var expiry) ||
            expiry.Month is < 1 or > 12 || new DateTime(2000 + expiry.Year, expiry.Month, 1).AddMonths(1) <= new DateTime(utcNow.Year, utcNow.Month, 1))
            return Invalid("Enter a future expiry date in MM/YY format.", card.Brand, Mask(normalizedNumber));

        if (cvv is null || cvv.Length != card.CvvLength || !cvv.All(char.IsDigit))
            return Invalid($"Enter the {card.CvvLength}-digit demo security code.", card.Brand, Mask(normalizedNumber));

        if (!string.IsNullOrWhiteSpace(postalCode) &&
            (postalCode.Trim().Length > 20 || !Regex.IsMatch(postalCode.Trim(), @"^[\p{L}\p{N}][\p{L}\p{N} .'-]*$", RegexOptions.CultureInvariant)))
            return Invalid("Enter a valid postal code or leave it blank.", card.Brand, Mask(normalizedNumber));

        return new DemoCardValidationResult(true, null, card.Brand, Mask(normalizedNumber), card.Outcome);
    }

    internal static bool IsLuhnValid(string digits)
    {
        var sum = 0;
        var alternate = false;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var digit = digits[i] - '0';
            if (alternate && (digit *= 2) > 9)
                digit -= 9;
            sum += digit;
            alternate = !alternate;
        }
        return sum % 10 == 0;
    }

    private static DemoCardValidationResult Invalid(string message, string? brand = null, string? masked = null) =>
        new(false, message, brand, masked, "Failed");

    private static string Mask(string digits) => $"{digits[..4]} **** **** {digits[^4..]}";
}
