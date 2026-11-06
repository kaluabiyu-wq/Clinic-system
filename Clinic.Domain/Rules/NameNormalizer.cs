namespace Clinic.Domain.Rules;

public static class NameNormalizer
{
    public static string Normalize(string name) =>
        string.Join(' ', name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
