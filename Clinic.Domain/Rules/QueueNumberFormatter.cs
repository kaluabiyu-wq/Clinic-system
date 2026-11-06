namespace Clinic.Domain.Rules;


public static class QueueNumberFormatter
{
    public static string Format(string queueLetter, int sequence) =>
        $"{queueLetter.Trim().ToUpperInvariant()}{sequence:000}";
}
