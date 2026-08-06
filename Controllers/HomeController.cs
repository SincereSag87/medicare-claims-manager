using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_claims_manager.Data;
using medicare_claims_manager.Models;

namespace medicare_claims_manager.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var openStatuses = new[] { ClaimStatus.Submitted, ClaimStatus.InReview, ClaimStatus.PendingDocumentation };

        var dashboard = new DashboardViewModel
        {
            TotalPatients = await _context.Patients.CountAsync(),
            TotalProviders = await _context.Providers.CountAsync(),
            OpenClaims = await _context.Claims.CountAsync(claim => openStatuses.Contains(claim.Status)),
            PaidClaims = await _context.Claims.CountAsync(claim => claim.Status == ClaimStatus.Paid),
            DeniedClaims = await _context.Claims.CountAsync(claim => claim.Status == ClaimStatus.Denied),
            PendingClaimValue = await _context.Claims
                .Where(claim => openStatuses.Contains(claim.Status))
                .SumAsync(claim => claim.BilledAmount),
            ApprovedClaimValue = await _context.Claims
                .Where(claim => claim.Status == ClaimStatus.Approved || claim.Status == ClaimStatus.Paid)
                .SumAsync(claim => claim.ApprovedAmount ?? 0),
            RecentClaims = await _context.Claims
                .Include(claim => claim.Patient)
                .Include(claim => claim.Provider)
                .OrderByDescending(claim => claim.UpdatedAt)
                .Take(5)
                .ToListAsync(),
            RecentWorkflowActivity = await _context.ClaimAuditEntries
                .AsNoTracking()
                .Include(entry => entry.Claim)
                .Where(entry => entry.Action == "Status Changed")
                .OrderByDescending(entry => entry.ChangedAt)
                .Take(4)
                .ToListAsync()
        };

        return View(dashboard);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
