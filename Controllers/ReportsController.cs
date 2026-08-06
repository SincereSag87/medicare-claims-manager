using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_claims_manager.Data;
using medicare_claims_manager.Models;

namespace medicare_claims_manager.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var openStatuses = new[] { ClaimStatus.Submitted, ClaimStatus.InReview, ClaimStatus.PendingDocumentation };

        var claims = await _context.Claims
            .AsNoTracking()
            .Include(claim => claim.Provider)
            .ToListAsync();

        var totalBilled = claims.Sum(claim => claim.BilledAmount);
        var totalApproved = claims.Sum(claim => claim.ApprovedAmount ?? 0);

        var report = new ReportsViewModel
        {
            TotalClaims = claims.Count,
            OpenClaims = claims.Count(claim => openStatuses.Contains(claim.Status)),
            PaidClaims = claims.Count(claim => claim.Status == ClaimStatus.Paid),
            DeniedClaims = claims.Count(claim => claim.Status == ClaimStatus.Denied),
            TotalBilled = totalBilled,
            TotalApproved = totalApproved,
            TotalOutstanding = claims
                .Where(claim => openStatuses.Contains(claim.Status))
                .Sum(claim => claim.BilledAmount - (claim.ApprovedAmount ?? 0)),
            ApprovalRate = totalBilled == 0 ? 0 : totalApproved / totalBilled,
            StatusBreakdown = claims
                .GroupBy(claim => claim.Status)
                .Select(group => new StatusReportItem
                {
                    Status = group.Key,
                    Count = group.Count(),
                    BilledAmount = group.Sum(claim => claim.BilledAmount)
                })
                .OrderBy(item => item.Status)
                .ToList(),
            PriorityBreakdown = claims
                .GroupBy(claim => claim.Priority)
                .Select(group => new PriorityReportItem
                {
                    Priority = group.Key,
                    Count = group.Count()
                })
                .OrderBy(item => item.Priority)
                .ToList(),
            ProviderPerformance = claims
                .GroupBy(claim => claim.Provider?.OrganizationName ?? "Unknown Provider")
                .Select(group => new ProviderReportItem
                {
                    ProviderName = group.Key,
                    ClaimCount = group.Count(),
                    BilledAmount = group.Sum(claim => claim.BilledAmount),
                    ApprovedAmount = group.Sum(claim => claim.ApprovedAmount ?? 0)
                })
                .OrderByDescending(item => item.BilledAmount)
                .Take(10)
                .ToList(),
            RecentWorkflowActivity = await _context.ClaimAuditEntries
                .AsNoTracking()
                .Include(entry => entry.Claim)
                .Where(entry => entry.Action == "Status Changed")
                .OrderByDescending(entry => entry.ChangedAt)
                .Take(10)
                .ToListAsync()
        };

        return View(report);
    }
}
