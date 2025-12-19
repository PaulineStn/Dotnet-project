using Gauniv.WebServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using X.PagedList;

namespace Gauniv.WebServer.Controllers;

[Authorize(Roles = "User")]
public class MyGamesController : Controller
{
    private readonly ApplicationDbContext _db;
    public MyGamesController(ApplicationDbContext db) => _db = db;


    public async Task<IActionResult> Index(int? categoryId, string? search, int page = 1, int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var query = _db.UserGamePurchases
            .Where(p => p.UserId == userId)
            .Include(p => p.Game)
                .ThenInclude(g => g.Categories)
            .Select(p => p.Game)
            .AsNoTracking()
            .AsQueryable();

        // Filtre catégorie
        if (categoryId.HasValue)
            query = query.Where(g => g.Categories.Any(c => c.Id == categoryId.Value));

        // Recherche nom
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(g => g.Name.ToLower().Contains(search.ToLower()));

        query = query.OrderBy(g => g.Id);

        // Dropdown catégories
        ViewBag.Categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

        var pagedGames = new StaticPagedList<Game>(items, page, pageSize, totalCount);


        // Optionnel : pour ré-afficher search/category dans la vue
        ViewBag.CurrentCategoryId = categoryId;
        ViewBag.CurrentSearch = search;

        return View(pagedGames); // Views/MyGames/Index.cshtml
    }

}
