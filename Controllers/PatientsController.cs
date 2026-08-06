using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_claims_manager.Data;
using medicare_claims_manager.Models;

namespace medicare_claims_manager.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PatientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var patientsQuery = _context.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            patientsQuery = patientsQuery.Where(patient =>
                patient.FirstName.Contains(term) ||
                patient.LastName.Contains(term) ||
                patient.MedicareNumber.Contains(term) ||
                (patient.Email != null && patient.Email.Contains(term)) ||
                (patient.Phone != null && patient.Phone.Contains(term)));
        }

        var patients = await patientsQuery
            .OrderBy(patient => patient.LastName)
            .ThenBy(patient => patient.FirstName)
            .ToListAsync();

        ViewData["Search"] = search;
        return View(patients);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var patient = await _context.Patients
            .AsNoTracking()
            .Include(patient => patient.Claims)
            .ThenInclude(claim => claim.Provider)
            .FirstOrDefaultAsync(patient => patient.Id == id);

        if (patient is null)
        {
            return NotFound();
        }

        return View(patient);
    }

    public IActionResult Create()
    {
        return View(new Patient { DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-65)) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FirstName,LastName,MedicareNumber,DateOfBirth,Email,Phone")] Patient patient)
    {
        await ValidateMedicareNumberAsync(patient);

        if (!ModelState.IsValid)
        {
            return View(patient);
        }

        _context.Add(patient);
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Patient record created.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var patient = await _context.Patients.FindAsync(id);
        if (patient is null)
        {
            return NotFound();
        }

        return View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,MedicareNumber,DateOfBirth,Email,Phone")] Patient patient)
    {
        if (id != patient.Id)
        {
            return NotFound();
        }

        await ValidateMedicareNumberAsync(patient);

        if (!ModelState.IsValid)
        {
            return View(patient);
        }

        try
        {
            _context.Update(patient);
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Patient record updated.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await PatientExistsAsync(patient.Id))
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

        var patient = await _context.Patients
            .AsNoTracking()
            .Include(patient => patient.Claims)
            .FirstOrDefaultAsync(patient => patient.Id == id);

        if (patient is null)
        {
            return NotFound();
        }

        return View(patient);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var patient = await _context.Patients
            .Include(patient => patient.Claims)
            .FirstOrDefaultAsync(patient => patient.Id == id);

        if (patient is null)
        {
            return NotFound();
        }

        if (patient.Claims.Any())
        {
            ModelState.AddModelError(string.Empty, "Patients with existing claims cannot be deleted.");
            return View("Delete", patient);
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Patient record deleted.";

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateMedicareNumberAsync(Patient patient)
    {
        patient.MedicareNumber = patient.MedicareNumber?.Trim() ?? string.Empty;
        patient.FirstName = patient.FirstName?.Trim() ?? string.Empty;
        patient.LastName = patient.LastName?.Trim() ?? string.Empty;
        patient.Email = string.IsNullOrWhiteSpace(patient.Email) ? null : patient.Email.Trim();
        patient.Phone = string.IsNullOrWhiteSpace(patient.Phone) ? null : patient.Phone.Trim();

        var duplicateExists = await _context.Patients.AnyAsync(existing =>
            existing.Id != patient.Id &&
            existing.MedicareNumber == patient.MedicareNumber);

        if (duplicateExists)
        {
            ModelState.AddModelError(nameof(Patient.MedicareNumber), "A patient with this Medicare number already exists.");
        }
    }

    private Task<bool> PatientExistsAsync(int id)
    {
        return _context.Patients.AnyAsync(patient => patient.Id == id);
    }
}
