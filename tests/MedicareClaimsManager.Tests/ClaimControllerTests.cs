using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_claims_manager.Controllers;
using medicare_claims_manager.Data;
using medicare_claims_manager.Models;

namespace MedicareClaimsManager.Tests;

public class ClaimControllerTests
{
    [Fact]
    public async Task Create_AddsDraftClaimAndAuditEntry_WhenModelIsValid()
    {
        await using var context = CreateContext();
        var patient = new Patient { FirstName = "Avery", LastName = "Demo", MedicareNumber = "DEMO-MBI-910", DateOfBirth = new DateOnly(1951, 1, 1) };
        var provider = new Provider { OrganizationName = "Demo Clinic", Npi = "1234567890", Specialty = "Primary Care", ContactEmail = "demo@example.test" };
        context.Patients.Add(patient);
        context.Providers.Add(provider);
        await context.SaveChangesAsync();
        var controller = ControllerTestHelper.PrepareController(new ClaimsController(context));

        var result = await controller.Create(new Claim
        {
            ClaimNumber = "CLM-TEST-100",
            PatientId = patient.Id,
            ProviderId = provider.Id,
            ServiceDate = new DateOnly(2026, 1, 15),
            BilledAmount = 125m,
            Priority = ClaimPriority.Standard,
            Notes = "Synthetic test claim"
        });

        Assert.IsType<RedirectToActionResult>(result);
        var claim = await context.Claims.Include(item => item.AuditEntries).SingleAsync();
        Assert.Equal(ClaimStatus.Draft, claim.Status);
        Assert.Single(claim.AuditEntries);
    }

    [Fact]
    public async Task ChangeStatus_RejectsInvalidTransition()
    {
        await using var context = CreateContext();
        var claim = new Claim
        {
            ClaimNumber = "CLM-TEST-101",
            PatientId = 1,
            ProviderId = 1,
            ServiceDate = new DateOnly(2026, 1, 15),
            BilledAmount = 125m,
            Status = ClaimStatus.Draft
        };
        context.Claims.Add(claim);
        await context.SaveChangesAsync();
        var controller = ControllerTestHelper.PrepareController(new ClaimsController(context));

        var result = await controller.ChangeStatus(claim.Id, ClaimStatus.Paid, "Invalid move");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(ClaimStatus.Draft, (await context.Claims.FindAsync(claim.Id))!.Status);
        Assert.Empty(context.ClaimAuditEntries);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"claims-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
