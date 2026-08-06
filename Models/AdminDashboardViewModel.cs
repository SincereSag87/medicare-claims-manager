using Microsoft.AspNetCore.Identity;

namespace medicare_claims_manager.Models;

public class AdminDashboardViewModel
{
    public int UserCount { get; set; }

    public int RoleCount { get; set; }

    public int PatientCount { get; set; }

    public int ProviderCount { get; set; }

    public int ClaimCount { get; set; }

    public IReadOnlyList<AdminUserViewModel> Users { get; set; } = Array.Empty<AdminUserViewModel>();

    public IReadOnlyList<IdentityRole> Roles { get; set; } = Array.Empty<IdentityRole>();
}

public class AdminUserViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public bool EmailConfirmed { get; set; }

    public bool LockoutEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}

public class EditUserRolesViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public IReadOnlyList<RoleSelectionViewModel> Roles { get; set; } = Array.Empty<RoleSelectionViewModel>();
}

public class RoleSelectionViewModel
{
    public string RoleName { get; set; } = string.Empty;

    public bool IsSelected { get; set; }
}
