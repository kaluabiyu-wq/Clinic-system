using Microsoft.AspNetCore.Identity;

namespace Clinic.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<string>
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    /// <summary>Set for Clinician accounts so call-next knows which doctor's queue to use.</summary>
    public string? DoctorId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
