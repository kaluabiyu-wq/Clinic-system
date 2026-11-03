using Clinic.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Clinic.Api.Auth;

public static class Policies
{
    public const string AdministratorOnly = "AdministratorOnly";
    public const string ReceptionOrAdmin = "ReceptionOrAdmin";
    public const string ClinicianOnly = "ClinicianOnly";

    public static void Register(AuthorizationOptions o)
    {
        o.AddPolicy(AdministratorOnly, p => p.RequireRole(RoleNames.Administrator));
        o.AddPolicy(ReceptionOrAdmin, p => p.RequireRole(RoleNames.Receptionist, RoleNames.Administrator));
        o.AddPolicy(ClinicianOnly, p => p.RequireRole(RoleNames.Clinician));
    }
}
