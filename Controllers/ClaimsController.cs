using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_claims_manager.Data;

namespace medicare_claims_manager.Controllers;

[Authorize]
public class ClaimsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClaimsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var claims = await _context.Claims
            .Include(claim => claim.Patient)
            .Include(claim => claim.Provider)
            .OrderByDescending(claim => claim.UpdatedAt)
            .ToListAsync();

        return View(claims);
    }
}
