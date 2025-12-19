using Gauniv.WebServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gauniv.WebServer.Controllers;

[Authorize(Roles = "User")]
public class MyGamesController : Controller
{
    private readonly ApplicationDbContext _db;
    public MyGamesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var games = await _db.UserGamePurchases
            .Where(p => p.UserId == userId)
            .Include(p => p.Game)
                .ThenInclude(g => g.Categories)
            .Select(p => p.Game)
            .AsNoTracking()
            .ToListAsync();

        return View(games); // 👈 va chercher Views/MyGames/Index.cshtml
    }
}
