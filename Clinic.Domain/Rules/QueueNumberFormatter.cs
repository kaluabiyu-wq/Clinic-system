namespace Clinic.Domain.Rules;

/// <summary>
/// Queue numbers are the doctor's letter plus three digits, written without a dash: G005.
/// (The roadmap used both G-005 and G005; this is the single format used everywhere.)
/// </summary>
public static class QueueNumberFormatter
{
    public static string Format(string queueLetter, int sequence) =>
        $"{queueLetter.Trim().ToUpperInvariant()}{sequence:000}";
}
