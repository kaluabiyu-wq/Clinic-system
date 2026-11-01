using System.Globalization;

namespace Clinic.Domain.Rules;

/// <summary>
/// Booking references look like APT1006. The number is the appointment Id plus 1000,
/// so it is unique, needs no second database write, and can be turned back into an Id.
/// </summary>
public static class ReferenceGenerator
{
    private const string Prefix = "APT";
    private const int Offset = 1000;

    public static string Format(int appointmentId) => $"{Prefix}{appointmentId + Offset}";

    public static bool TryParseId(string? reference, out int appointmentId)
    {
        appointmentId = 0;

        if (string.IsNullOrWhiteSpace(reference))
            return false;

        var text = reference.Trim().ToUpperInvariant();

        if (!text.StartsWith(Prefix, StringComparison.Ordinal))
            return false;

        if (!int.TryParse(text[Prefix.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out var number))
            return false;

        appointmentId = number - Offset;
        return appointmentId > 0;
    }
}
