using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_claims_manager.Data;
using medicare_claims_manager.Models;

namespace medicare_claims_manager.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .ToListAsync();

        var userViewModels = new List<AdminUserViewModel>();
        foreach (var user in users)
        {
            userViewModels.Add(new AdminUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                Roles = (await _userManager.GetRolesAsync(user)).OrderBy(role => role).ToList()
            });
        }

        var roles = await _roleManager.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .ToListAsync();

        return View(new AdminDashboardViewModel
        {
            UserCount = users.Count,
            RoleCount = roles.Count,
            PatientCount = await _context.Patients.CountAsync(),
            ProviderCount = await _context.Providers.CountAsync(),
            ClaimCount = await _context.Claims.CountAsync(),
            Users = userViewModels,
            Roles = roles
        });
    }

    public async Task<IActionResult> EditRoles(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var assignedRoles = await _userManager.GetRolesAsync(user);
        var roles = await _roleManager.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .Select(role => role.Name!)
            .ToListAsync();

        return View(new EditUserRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName ?? string.Empty,
            Roles = roles.Select(role => new RoleSelectionViewModel
            {
                RoleName = role,
                IsSelected = assignedRoles.Contains(role)
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRoles(EditUserRolesViewModel model, string[] selectedRoles)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null)
        {
            return NotFound();
        }

        var validRoles = await _roleManager.Roles
            .Select(role => role.Name!)
            .ToListAsync();

        var requestedRoles = selectedRoles
            .Where(role => validRoles.Contains(role))
            .Distinct()
            .ToList();

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(requestedRoles));
        if (!removeResult.Succeeded)
        {
            AddIdentityErrors(removeResult);
            return await RebuildEditRolesViewAsync(user, requestedRoles);
        }

        var addResult = await _userManager.AddToRolesAsync(user, requestedRoles.Except(currentRoles));
        if (!addResult.Succeeded)
        {
            AddIdentityErrors(addResult);
            return await RebuildEditRolesViewAsync(user, requestedRoles);
        }

        TempData["StatusMessage"] = "User roles updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmEmail(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
        TempData["StatusMessage"] = "User email marked as confirmed.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> RebuildEditRolesViewAsync(IdentityUser user, IReadOnlyCollection<string> selectedRoles)
    {
        var roles = await _roleManager.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .Select(role => role.Name!)
            .ToListAsync();

        return View(new EditUserRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName ?? string.Empty,
            Roles = roles.Select(role => new RoleSelectionViewModel
            {
                RoleName = role,
                IsSelected = selectedRoles.Contains(role)
            }).ToList()
        });
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
