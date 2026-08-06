using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_claims_manager.Data;
using medicare_claims_manager.Models;

namespace medicare_claims_manager.Controllers;

[Authorize]
public class ProvidersController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProvidersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var providersQuery = _context.Providers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            providersQuery = providersQuery.Where(provider =>
                provider.OrganizationName.Contains(term) ||
                provider.Npi.Contains(term) ||
                provider.Specialty.Contains(term) ||
                provider.ContactEmail.Contains(term));
        }

        var providers = await providersQuery
            .OrderBy(provider => provider.OrganizationName)
            .ToListAsync();

        ViewData["Search"] = search;
        return View(providers);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var provider = await _context.Providers
            .AsNoTracking()
            .Include(provider => provider.Claims)
            .ThenInclude(claim => claim.Patient)
            .FirstOrDefaultAsync(provider => provider.Id == id);

        if (provider is null)
        {
            return NotFound();
        }

        return View(provider);
    }

    public IActionResult Create()
    {
        return View(new Provider());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("OrganizationName,Npi,Specialty,ContactEmail")] Provider provider)
    {
        await ValidateProviderAsync(provider);

        if (!ModelState.IsValid)
        {
            return View(provider);
        }

        _context.Add(provider);
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Provider record created.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var provider = await _context.Providers.FindAsync(id);
        if (provider is null)
        {
            return NotFound();
        }

        return View(provider);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,OrganizationName,Npi,Specialty,ContactEmail")] Provider provider)
    {
        if (id != provider.Id)
        {
            return NotFound();
        }

        await ValidateProviderAsync(provider);

        if (!ModelState.IsValid)
        {
            return View(provider);
        }

        try
        {
            _context.Update(provider);
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Provider record updated.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProviderExistsAsync(provider.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var provider = await _context.Providers
            .AsNoTracking()
            .Include(provider => provider.Claims)
            .FirstOrDefaultAsync(provider => provider.Id == id);

        if (provider is null)
        {
            return NotFound();
        }

        return View(provider);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var provider = await _context.Providers
            .Include(provider => provider.Claims)
            .FirstOrDefaultAsync(provider => provider.Id == id);

        if (provider is null)
        {
            return NotFound();
        }

        if (provider.Claims.Any())
        {
            ModelState.AddModelError(string.Empty, "Providers with existing claims cannot be deleted.");
            return View("Delete", provider);
        }

        _context.Providers.Remove(provider);
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Provider record deleted.";

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateProviderAsync(Provider provider)
    {
        provider.OrganizationName = provider.OrganizationName?.Trim() ?? string.Empty;
        provider.Npi = provider.Npi?.Trim() ?? string.Empty;
        provider.Specialty = provider.Specialty?.Trim() ?? string.Empty;
        provider.ContactEmail = provider.ContactEmail?.Trim() ?? string.Empty;

        var duplicateExists = await _context.Providers.AnyAsync(existing =>
            existing.Id != provider.Id &&
            existing.Npi == provider.Npi);

        if (duplicateExists)
        {
            ModelState.AddModelError(nameof(Provider.Npi), "A provider with this NPI already exists.");
        }
    }

    private Task<bool> ProviderExistsAsync(int id)
    {
        return _context.Providers.AnyAsync(provider => provider.Id == id);
    }
}
