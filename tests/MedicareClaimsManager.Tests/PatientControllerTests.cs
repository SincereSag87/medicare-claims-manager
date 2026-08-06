using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_claims_manager.Controllers;
using medicare_claims_manager.Data;
using medicare_claims_manager.Models;

namespace MedicareClaimsManager.Tests;

public class PatientControllerTests
{
    [Fact]
    public async Task Create_AddsPatient_WhenModelIsValid()
    {
        await using var context = CreateContext();
        var controller = ControllerTestHelper.PrepareController(new PatientsController(context));
        var patient = new Patient
        {
            FirstName = "Taylor",
            LastName = "Morris",
            MedicareNumber = "DEMO-MBI-900",
            DateOfBirth = new DateOnly(1950, 5, 15),
            Email = "taylor.morris@example.test",
            Phone = "216-555-0199"
        };

        var result = await controller.Create(patient);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(context.Patients);
    }

    [Fact]
    public async Task Create_ReturnsView_WhenMedicareNumberIsDuplicate()
    {
        await using var context = CreateContext();
        context.Patients.Add(new Patient
        {
            FirstName = "Existing",
            LastName = "Patient",
            MedicareNumber = "DEMO-MBI-901",
            DateOfBirth = new DateOnly(1949, 3, 10)
        });
        await context.SaveChangesAsync();
        var controller = ControllerTestHelper.PrepareController(new PatientsController(context));

        var result = await controller.Create(new Patient
        {
            FirstName = "New",
            LastName = "Patient",
            MedicareNumber = "DEMO-MBI-901",
            DateOfBirth = new DateOnly(1952, 8, 1)
        });

        Assert.IsType<ViewResult>(result);
        Assert.Equal(1, await context.Patients.CountAsync());
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"patients-{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
