using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_claims_manager.Data;

namespace medicare_claims_manager.Controllers;

[Authorize]
public class ProvidersController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProvidersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var providers = await _context.Providers
            .OrderBy(provider => provider.OrganizationName)
            .ToListAsync();

        return View(providers);
    }
}
