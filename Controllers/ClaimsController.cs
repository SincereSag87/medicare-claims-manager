using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using medicare_claims_manager.Data;
using medicare_claims_manager.Models;

namespace medicare_claims_manager.Controllers;

[Authorize]
public class ClaimsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClaimsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, ClaimStatus? status)
    {
        var claimsQuery = _context.Claims
            .AsNoTracking()
            .Include(claim => claim.Patient)
            .Include(claim => claim.Provider)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            claimsQuery = claimsQuery.Where(claim =>
                claim.ClaimNumber.Contains(term) ||
                claim.Patient!.FirstName.Contains(term) ||
                claim.Patient.LastName.Contains(term) ||
                claim.Provider!.OrganizationName.Contains(term));
        }

        if (status.HasValue)
        {
            claimsQuery = claimsQuery.Where(claim => claim.Status == status.Value);
        }

        var claims = await claimsQuery
            .OrderByDescending(claim => claim.UpdatedAt)
            .ToListAsync();

        ViewData["Search"] = search;
        ViewData["Status"] = status;
        return View(claims);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var claim = await _context.Claims
            .AsNoTracking()
            .Include(claim => claim.Patient)
            .Include(claim => claim.Provider)
            .FirstOrDefaultAsync(claim => claim.Id == id);

        if (claim is null)
        {
            return NotFound();
        }

        return View(claim);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View(new Claim
        {
            ServiceDate = DateOnly.FromDateTime(DateTime.Today),
            Status = ClaimStatus.Draft,
            Priority = ClaimPriority.Standard
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ClaimNumber,PatientId,ProviderId,ServiceDate,BilledAmount,ApprovedAmount,Status,Priority,Notes")] Claim claim)
    {
        await ValidateClaimAsync(claim);

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(claim.PatientId, claim.ProviderId);
            return View(claim);
        }

        claim.CreatedAt = DateTimeOffset.UtcNow;
        claim.UpdatedAt = claim.CreatedAt;

        _context.Add(claim);
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Claim record created.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var claim = await _context.Claims.FindAsync(id);
        if (claim is null)
        {
            return NotFound();
        }

        await PopulateLookupsAsync(claim.PatientId, claim.ProviderId);
        return View(claim);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ClaimNumber,PatientId,ProviderId,ServiceDate,BilledAmount,ApprovedAmount,Status,Priority,Notes")] Claim claim)
    {
        if (id != claim.Id)
        {
            return NotFound();
        }

        await ValidateClaimAsync(claim);

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(claim.PatientId, claim.ProviderId);
            return View(claim);
        }

        var existingClaim = await _context.Claims.FindAsync(id);
        if (existingClaim is null)
        {
            return NotFound();
        }

        existingClaim.ClaimNumber = claim.ClaimNumber;
        existingClaim.PatientId = claim.PatientId;
        existingClaim.ProviderId = claim.ProviderId;
        existingClaim.ServiceDate = claim.ServiceDate;
        existingClaim.BilledAmount = claim.BilledAmount;
        existingClaim.ApprovedAmount = claim.ApprovedAmount;
        existingClaim.Status = claim.Status;
        existingClaim.Priority = claim.Priority;
        existingClaim.Notes = claim.Notes;
        existingClaim.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Claim record updated.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var claim = await _context.Claims
            .AsNoTracking()
            .Include(claim => claim.Patient)
            .Include(claim => claim.Provider)
            .FirstOrDefaultAsync(claim => claim.Id == id);

        if (claim is null)
        {
            return NotFound();
        }

        return View(claim);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var claim = await _context.Claims.FindAsync(id);

        if (claim is null)
        {
            return NotFound();
        }

        _context.Claims.Remove(claim);
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Claim record deleted.";

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(int? selectedPatientId = null, int? selectedProviderId = null)
    {
        var patients = await _context.Patients
            .AsNoTracking()
            .OrderBy(patient => patient.LastName)
            .ThenBy(patient => patient.FirstName)
            .Select(patient => new
            {
                patient.Id,
                Name = patient.LastName + ", " + patient.FirstName + " - " + patient.MedicareNumber
            })
            .ToListAsync();

        var providers = await _context.Providers
            .AsNoTracking()
            .OrderBy(provider => provider.OrganizationName)
            .Select(provider => new
            {
                provider.Id,
                Name = provider.OrganizationName + " - " + provider.Npi
            })
            .ToListAsync();

        ViewData["PatientId"] = new SelectList(patients, "Id", "Name", selectedPatientId);
        ViewData["ProviderId"] = new SelectList(providers, "Id", "Name", selectedProviderId);
    }

    private async Task ValidateClaimAsync(Claim claim)
    {
        claim.ClaimNumber = claim.ClaimNumber?.Trim() ?? string.Empty;
        claim.Notes = string.IsNullOrWhiteSpace(claim.Notes) ? null : claim.Notes.Trim();

        var duplicateExists = await _context.Claims.AnyAsync(existing =>
            existing.Id != claim.Id &&
            existing.ClaimNumber == claim.ClaimNumber);

        if (duplicateExists)
        {
            ModelState.AddModelError(nameof(Claim.ClaimNumber), "A claim with this claim number already exists.");
        }

        if (!await _context.Patients.AnyAsync(patient => patient.Id == claim.PatientId))
        {
            ModelState.AddModelError(nameof(Claim.PatientId), "Select a valid patient.");
        }

        if (!await _context.Providers.AnyAsync(provider => provider.Id == claim.ProviderId))
        {
            ModelState.AddModelError(nameof(Claim.ProviderId), "Select a valid provider.");
        }

        if (claim.ApprovedAmount.HasValue && claim.ApprovedAmount > claim.BilledAmount)
        {
            ModelState.AddModelError(nameof(Claim.ApprovedAmount), "Approved amount cannot exceed billed amount.");
        }
    }
}
