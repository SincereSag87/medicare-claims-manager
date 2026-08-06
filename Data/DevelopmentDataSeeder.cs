using Microsoft.EntityFrameworkCore;
using medicare_claims_manager.Models;

namespace medicare_claims_manager.Data;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        if (!configuration.GetValue("SeedData:Enabled", false))
        {
            return;
        }

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await context.Claims.AnyAsync() || await context.Patients.AnyAsync() || await context.Providers.AnyAsync())
        {
            return;
        }

        var patients = new[]
        {
            new Patient
            {
                FirstName = "Avery",
                LastName = "Johnson",
                MedicareNumber = "DEMO-MBI-001",
                DateOfBirth = new DateOnly(1951, 4, 12),
                Phone = "216-555-0101",
                Email = "avery.johnson@example.test"
            },
            new Patient
            {
                FirstName = "Morgan",
                LastName = "Ellis",
                MedicareNumber = "DEMO-MBI-002",
                DateOfBirth = new DateOnly(1948, 9, 3),
                Phone = "216-555-0102",
                Email = "morgan.ellis@example.test"
            },
            new Patient
            {
                FirstName = "Riley",
                LastName = "Carter",
                MedicareNumber = "DEMO-MBI-003",
                DateOfBirth = new DateOnly(1956, 1, 22),
                Phone = "216-555-0103",
                Email = "riley.carter@example.test"
            },
            new Patient
            {
                FirstName = "Jordan",
                LastName = "Nguyen",
                MedicareNumber = "DEMO-MBI-004",
                DateOfBirth = new DateOnly(1953, 7, 18),
                Phone = "216-555-0104",
                Email = "jordan.nguyen@example.test"
            }
        };

        var providers = new[]
        {
            new Provider
            {
                OrganizationName = "Lakeview Cardiology Group",
                Npi = "1000000001",
                Specialty = "Cardiology",
                ContactEmail = "billing@lakeview-cardio.example.test"
            },
            new Provider
            {
                OrganizationName = "North Coast Primary Care",
                Npi = "1000000002",
                Specialty = "Primary Care",
                ContactEmail = "claims@northcoast-primary.example.test"
            },
            new Provider
            {
                OrganizationName = "Harbor Rehabilitation Center",
                Npi = "1000000003",
                Specialty = "Physical Therapy",
                ContactEmail = "revenue@harbor-rehab.example.test"
            }
        };

        await context.Patients.AddRangeAsync(patients);
        await context.Providers.AddRangeAsync(providers);
        await context.SaveChangesAsync();

        var now = DateTimeOffset.UtcNow;
        var claims = new[]
        {
            CreateClaim("CLM-DEMO-1001", patients[0], providers[0], now.AddDays(-18), ClaimStatus.Submitted, ClaimPriority.High, 1850.00m, null, "Awaiting payer review."),
            CreateClaim("CLM-DEMO-1002", patients[1], providers[1], now.AddDays(-14), ClaimStatus.InReview, ClaimPriority.Standard, 420.00m, null, "Eligibility verified; coding review in progress."),
            CreateClaim("CLM-DEMO-1003", patients[2], providers[2], now.AddDays(-10), ClaimStatus.PendingDocumentation, ClaimPriority.Urgent, 975.50m, null, "Therapy notes requested from provider."),
            CreateClaim("CLM-DEMO-1004", patients[3], providers[0], now.AddDays(-8), ClaimStatus.Approved, ClaimPriority.Standard, 2310.00m, 1985.75m, "Approved pending payment posting."),
            CreateClaim("CLM-DEMO-1005", patients[0], providers[1], now.AddDays(-6), ClaimStatus.Denied, ClaimPriority.High, 315.25m, 0m, "Denied for missing referral documentation."),
            CreateClaim("CLM-DEMO-1006", patients[1], providers[2], now.AddDays(-3), ClaimStatus.Paid, ClaimPriority.Standard, 740.00m, 702.00m, "Payment posted and claim closed.")
        };

        await context.Claims.AddRangeAsync(claims);
        await context.SaveChangesAsync();

        var auditEntries = claims.SelectMany(claim => BuildAuditTrail(claim, now)).ToList();
        await context.ClaimAuditEntries.AddRangeAsync(auditEntries);
        await context.SaveChangesAsync();
    }

    private static Claim CreateClaim(
        string claimNumber,
        Patient patient,
        Provider provider,
        DateTimeOffset serviceDate,
        ClaimStatus status,
        ClaimPriority priority,
        decimal billedAmount,
        decimal? approvedAmount,
        string notes)
    {
        return new Claim
        {
            ClaimNumber = claimNumber,
            PatientId = patient.Id,
            ProviderId = provider.Id,
            ServiceDate = DateOnly.FromDateTime(serviceDate.Date),
            BilledAmount = billedAmount,
            ApprovedAmount = approvedAmount,
            Status = status,
            Priority = priority,
            Notes = notes,
            CreatedAt = serviceDate.AddDays(1),
            UpdatedAt = serviceDate.AddDays(2)
        };
    }

    private static IEnumerable<ClaimAuditEntry> BuildAuditTrail(Claim claim, DateTimeOffset now)
    {
        var createdAt = claim.CreatedAt;
        yield return CreateAuditEntry(claim.Id, "Created", null, null, "Draft", "Synthetic demo claim created.", createdAt);

        if (claim.Status == ClaimStatus.Draft)
        {
            yield break;
        }

        yield return CreateAuditEntry(claim.Id, "Status Changed", nameof(Claim.Status), "Draft", "Submitted", "Submitted to payer queue.", createdAt.AddHours(4));

        if (claim.Status is ClaimStatus.InReview or ClaimStatus.PendingDocumentation or ClaimStatus.Approved or ClaimStatus.Denied or ClaimStatus.Paid)
        {
            yield return CreateAuditEntry(claim.Id, "Status Changed", nameof(Claim.Status), "Submitted", "InReview", "Billing specialist review started.", createdAt.AddDays(1));
        }

        if (claim.Status == ClaimStatus.PendingDocumentation)
        {
            yield return CreateAuditEntry(claim.Id, "Status Changed", nameof(Claim.Status), "InReview", "PendingDocumentation", "Requested missing documentation.", createdAt.AddDays(2));
        }

        if (claim.Status is ClaimStatus.Approved or ClaimStatus.Paid)
        {
            yield return CreateAuditEntry(claim.Id, "Status Changed", nameof(Claim.Status), "InReview", "Approved", "Claim approved for reimbursement.", createdAt.AddDays(2));
            yield return CreateAuditEntry(claim.Id, "Field Updated", nameof(Claim.ApprovedAmount), null, claim.ApprovedAmount?.ToString("F2"), "Approved amount recorded.", createdAt.AddDays(2).AddHours(2));
        }

        if (claim.Status == ClaimStatus.Denied)
        {
            yield return CreateAuditEntry(claim.Id, "Status Changed", nameof(Claim.Status), "InReview", "Denied", "Claim denied after review.", createdAt.AddDays(2));
        }

        if (claim.Status == ClaimStatus.Paid)
        {
            yield return CreateAuditEntry(claim.Id, "Status Changed", nameof(Claim.Status), "Approved", "Paid", "Payment posted.", now.AddDays(-1));
        }
    }

    private static ClaimAuditEntry CreateAuditEntry(
        int claimId,
        string action,
        string? fieldName,
        string? oldValue,
        string? newValue,
        string? notes,
        DateTimeOffset changedAt)
    {
        return new ClaimAuditEntry
        {
            ClaimId = claimId,
            Action = action,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            Notes = notes,
            ChangedBy = "Demo Seeder",
            ChangedAt = changedAt
        };
    }
}
