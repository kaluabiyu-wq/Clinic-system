using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Users;
using Clinic.Domain.Common;
using Clinic.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Identity;

public class StaffUserStore : IStaffUserStore
{
    private readonly UserManager<ApplicationUser> _users;

    public StaffUserStore(UserManager<ApplicationUser> users) => _users = users;

    public async Task<IReadOnlyList<StaffUserDto>> ListAsync(CancellationToken ct)
    {
        var all = await _users.Users.AsNoTracking().OrderBy(u => u.FullName).ToListAsync(ct);
        var result = new List<StaffUserDto>(all.Count);
        // Staff lists are small, so one role lookup per user is acceptable here.
        foreach (var u in all) result.Add(await ToDtoAsync(u));
        return result;
    }

    public async Task<StaffUserDto?> FindAsync(string id, CancellationToken ct)
    {
        var u = await _users.FindByIdAsync(id.ToString());
        return u is null ? null : await ToDtoAsync(u);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct) =>
        await _users.FindByEmailAsync(email.Trim()) is not null;

    public async Task<int> CountActiveAdministratorsAsync(CancellationToken ct) =>
        (await _users.GetUsersInRoleAsync(RoleNames.Administrator)).Count(u => u.IsActive);

    public async Task<StaffUserDto> CreateAsync(CreateStaffUserDto dto, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true,
            FullName = dto.FullName.Trim(),
            DoctorId = dto.DoctorId,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        Throw(await _users.CreateAsync(user, dto.Password));
        var roleResult = await _users.AddToRoleAsync(user, dto.Role);
        if (!roleResult.Succeeded)
        {
            await _users.DeleteAsync(user); // do not leave a role-less account behind
            Throw(roleResult);
        }
        return await ToDtoAsync(user);
    }

    public async Task<StaffUserDto> UpdateAsync(string id, UpdateStaffUserDto dto, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(id.ToString())
            ?? throw new NotFoundException("Staff account not found.");

        user.FullName = dto.FullName.Trim();
        user.DoctorId = dto.DoctorId;
        user.IsActive = dto.IsActive;
        // Lockout makes deactivation effective at the identity layer; AuthService also checks IsActive.
        user.LockoutEnabled = true;
        user.LockoutEnd = dto.IsActive ? null : DateTimeOffset.MaxValue;
        Throw(await _users.UpdateAsync(user));

        var currentRoles = await _users.GetRolesAsync(user);
        if (currentRoles.Count != 1 || currentRoles[0] != dto.Role)
        {
            Throw(await _users.RemoveFromRolesAsync(user, currentRoles));
            Throw(await _users.AddToRoleAsync(user, dto.Role));
            await _users.UpdateSecurityStampAsync(user);
        }

        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            var token = await _users.GeneratePasswordResetTokenAsync(user);
            Throw(await _users.ResetPasswordAsync(user, token, dto.NewPassword));
        }

        return await ToDtoAsync(user);
    }

    private async Task<StaffUserDto> ToDtoAsync(ApplicationUser u)
    {
        var roles = await _users.GetRolesAsync(u);
        return new StaffUserDto(u.Id, u.FullName, u.Email ?? "", roles.FirstOrDefault() ?? "", u.IsActive, u.DoctorId, u.CreatedAtUtc);
    }

    private static void Throw(IdentityResult r)
    {
        if (!r.Succeeded)
            throw new DomainException(string.Join(" ", r.Errors.Select(e => e.Description)));
    }
}
