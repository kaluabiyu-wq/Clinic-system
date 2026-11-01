namespace Clinic.Domain.Rules;

public static class NameNormalizer
{
    /// <summary>Trims and collapses repeated spaces: "  Abebe   Kebede " becomes "Abebe Kebede".</summary>
    public static string Normalize(string name) =>
        string.Join(' ', name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
