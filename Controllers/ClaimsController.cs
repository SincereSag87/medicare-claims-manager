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
            .Include(claim => claim.AuditEntries.OrderByDescending(entry => entry.ChangedAt))
            .FirstOrDefaultAsync(claim => claim.Id == id);

        if (claim is null)
        {
            return NotFound();
        }

        return View(new ClaimDetailsViewModel
        {
            Claim = claim,
            NextStatuses = ClaimStatusWorkflow.GetNextStatuses(claim.Status)
        });
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
    public async Task<IActionResult> Create([Bind("ClaimNumber,PatientId,ProviderId,ServiceDate,BilledAmount,ApprovedAmount,Priority,Notes")] Claim claim)
    {
        claim.Status = ClaimStatus.Draft;
        await ValidateClaimAsync(claim);

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(claim.PatientId, claim.ProviderId);
            return View(claim);
        }

        claim.CreatedAt = DateTimeOffset.UtcNow;
        claim.UpdatedAt = claim.CreatedAt;
        claim.AuditEntries.Add(CreateAuditEntry("Created", null, null, "Draft", "Claim intake record created."));

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
    public async Task<IActionResult> Edit(int id, [Bind("Id,ClaimNumber,PatientId,ProviderId,ServiceDate,BilledAmount,ApprovedAmount,Priority,Notes")] Claim claim)
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

        var auditEntries = BuildEditAuditEntries(existingClaim, claim);

        existingClaim.ClaimNumber = claim.ClaimNumber;
        existingClaim.PatientId = claim.PatientId;
        existingClaim.ProviderId = claim.ProviderId;
        existingClaim.ServiceDate = claim.ServiceDate;
        existingClaim.BilledAmount = claim.BilledAmount;
        existingClaim.ApprovedAmount = claim.ApprovedAmount;
        existingClaim.Priority = claim.Priority;
        existingClaim.Notes = claim.Notes;

        if (auditEntries.Count > 0)
        {
            existingClaim.UpdatedAt = DateTimeOffset.UtcNow;
            _context.ClaimAuditEntries.AddRange(auditEntries);
        }

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = auditEntries.Count > 0 ? "Claim record updated." : "No claim changes were made.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, ClaimStatus nextStatus, string? notes)
    {
        var claim = await _context.Claims.FindAsync(id);
        if (claim is null)
        {
            return NotFound();
        }

        if (!ClaimStatusWorkflow.CanTransition(claim.Status, nextStatus))
        {
            TempData["StatusMessage"] = $"Cannot move claim from {claim.Status} to {nextStatus}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var oldStatus = claim.Status;
        claim.Status = nextStatus;
        claim.UpdatedAt = DateTimeOffset.UtcNow;

        _context.ClaimAuditEntries.Add(CreateAuditEntry(
            "Status Changed",
            nameof(Claim.Status),
            oldStatus.ToString(),
            nextStatus.ToString(),
            string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            claim.Id));

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = $"Claim status changed to {nextStatus}.";

        return RedirectToAction(nameof(Details), new { id });
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

    private List<ClaimAuditEntry> BuildEditAuditEntries(Claim existingClaim, Claim submittedClaim)
    {
        var entries = new List<ClaimAuditEntry>();

        AddAuditEntryIfChanged(entries, existingClaim.Id, nameof(Claim.ClaimNumber), existingClaim.ClaimNumber, submittedClaim.ClaimNumber);
        AddAuditEntryIfChanged(entries, existingClaim.Id, nameof(Claim.PatientId), existingClaim.PatientId.ToString(), submittedClaim.PatientId.ToString());
        AddAuditEntryIfChanged(entries, existingClaim.Id, nameof(Claim.ProviderId), existingClaim.ProviderId.ToString(), submittedClaim.ProviderId.ToString());
        AddAuditEntryIfChanged(entries, existingClaim.Id, nameof(Claim.ServiceDate), existingClaim.ServiceDate.ToString("yyyy-MM-dd"), submittedClaim.ServiceDate.ToString("yyyy-MM-dd"));
        AddAuditEntryIfChanged(entries, existingClaim.Id, nameof(Claim.BilledAmount), existingClaim.BilledAmount.ToString("F2"), submittedClaim.BilledAmount.ToString("F2"));
        AddAuditEntryIfChanged(entries, existingClaim.Id, nameof(Claim.ApprovedAmount), existingClaim.ApprovedAmount?.ToString("F2"), submittedClaim.ApprovedAmount?.ToString("F2"));
        AddAuditEntryIfChanged(entries, existingClaim.Id, nameof(Claim.Priority), existingClaim.Priority.ToString(), submittedClaim.Priority.ToString());
        AddAuditEntryIfChanged(entries, existingClaim.Id, nameof(Claim.Notes), existingClaim.Notes, submittedClaim.Notes);

        return entries;
    }

    private void AddAuditEntryIfChanged(List<ClaimAuditEntry> entries, int claimId, string fieldName, string? oldValue, string? newValue)
    {
        if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            return;
        }

        entries.Add(CreateAuditEntry("Field Updated", fieldName, oldValue, newValue, null, claimId));
    }

    private ClaimAuditEntry CreateAuditEntry(string action, string? fieldName, string? oldValue, string? newValue, string? notes, int claimId = 0)
    {
        return new ClaimAuditEntry
        {
            ClaimId = claimId,
            Action = action,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            Notes = notes,
            ChangedBy = User.Identity?.Name ?? "System",
            ChangedAt = DateTimeOffset.UtcNow
        };
    }
}
