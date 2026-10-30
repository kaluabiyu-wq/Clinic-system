using Clinic.Domain.Common;

namespace Clinic.Domain.Rules;

/// <summary>
/// Normalizes Ethiopian mobile numbers to +2519XXXXXXXX / +2517XXXXXXXX.
/// Accepts 09XXXXXXXX, 07XXXXXXXX, 2519XXXXXXXX, +2519XXXXXXXX and 9XXXXXXXX,
/// with spaces or dashes.
/// </summary>
public static class PhoneNumberNormalizer
{
    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var digits = new string(input.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("251") && digits.Length == 12)
            digits = digits[3..];
        else if (digits.StartsWith('0') && digits.Length == 10)
            digits = digits[1..];
        else if (digits.Length != 9)
            return false;

        if (digits[0] != '9' && digits[0] != '7')
            return false;

        normalized = "+251" + digits;
        return true;
    }

    public static string Normalize(string input) =>
        TryNormalize(input, out var normalized)
            ? normalized
            : throw new DomainException("Invalid phone number.");
}
