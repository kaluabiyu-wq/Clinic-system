using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Users;
using Clinic.Domain.Common;
using Clinic.Domain.Constants;
using FluentValidation;

namespace Clinic.Application.Services;

public class UserService
{
    private readonly IStaffUserStore _store;
    private readonly ICurrentUser _current;
    private readonly IValidator<CreateStaffUserDto> _createValidator;
    private readonly IValidator<UpdateStaffUserDto> _updateValidator;

    public UserService(
        IStaffUserStore store,
        ICurrentUser current,
        IValidator<CreateStaffUserDto> createValidator,
        IValidator<UpdateStaffUserDto> updateValidator)
    {
        _store = store;
        _current = current;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public Task<IReadOnlyList<StaffUserDto>> ListAsync(CancellationToken ct) => _store.ListAsync(ct);

    public async Task<StaffUserDto> CreateAsync(CreateStaffUserDto dto, CancellationToken ct)
    {
        await _createValidator.ValidateAndThrowAsync(dto, ct);

        if (await _store.EmailExistsAsync(dto.Email, ct))
            throw new ConflictException("A staff account with this email already exists.");

        return await _store.CreateAsync(dto with { Email = dto.Email.Trim() }, ct);
    }

    public async Task<StaffUserDto> UpdateAsync(string id, UpdateStaffUserDto dto, CancellationToken ct)
    {
        await _updateValidator.ValidateAndThrowAsync(dto, ct);

        var existing = await _store.FindAsync(id, ct)
            ?? throw new NotFoundException("Staff account not found.");

        var isSelf = _current.UserId == id;
        if (isSelf && !dto.IsActive)
            throw new DomainException("You cannot deactivate your own account.");
        if (isSelf && dto.Role != existing.Role)
            throw new DomainException("You cannot change your own role.");

        // Never allow the system to end up with no active administrator.
        var losesAdminAccess = existing.Role == RoleNames.Administrator && existing.IsActive
            && (dto.Role != RoleNames.Administrator || !dto.IsActive);
        if (losesAdminAccess && await _store.CountActiveAdministratorsAsync(ct) <= 1)
            throw new ConflictException("At least one active administrator must remain.");

        return await _store.UpdateAsync(id, dto, ct);
    }
}
