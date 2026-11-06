using Microsoft.AspNetCore.Identity;

namespace Clinic.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<string>
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    public string? DoctorId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
