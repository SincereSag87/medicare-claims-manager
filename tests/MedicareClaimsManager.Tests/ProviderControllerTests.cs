using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_claims_manager.Controllers;
using medicare_claims_manager.Data;
using medicare_claims_manager.Models;

namespace MedicareClaimsManager.Tests;

public class ProviderControllerTests
{
    [Fact]
    public async Task Create_AddsProvider_WhenModelIsValid()
    {
        await using var context = CreateContext();
        var controller = ControllerTestHelper.PrepareController(new ProvidersController(context));

        var result = await controller.Create(new Provider
        {
            OrganizationName = "Integration Test Clinic",
            Npi = "1234567890",
            Specialty = "Primary Care",
            ContactEmail = "claims@example.test"
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(context.Providers);
    }

    [Fact]
    public async Task Create_ReturnsView_WhenNpiIsDuplicate()
    {
        await using var context = CreateContext();
        context.Providers.Add(new Provider
        {
            OrganizationName = "Existing Clinic",
            Npi = "1234567891",
            Specialty = "Cardiology",
            ContactEmail = "existing@example.test"
        });
        await context.SaveChangesAsync();
        var controller = ControllerTestHelper.PrepareController(new ProvidersController(context));

        var result = await controller.Create(new Provider
        {
            OrganizationName = "Duplicate Clinic",
            Npi = "1234567891",
            Specialty = "Cardiology",
            ContactEmail = "duplicate@example.test"
        });

        Assert.IsType<ViewResult>(result);
        Assert.Equal(1, await context.Providers.CountAsync());
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"providers-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
