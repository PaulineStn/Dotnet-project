using Gauniv.WebServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gauniv.WebServer.Controllers;

public class GamesController : Controller
{
    private readonly ApplicationDbContext _db;
    public GamesController(ApplicationDbContext db) => _db = db;

    [AllowAnonymous]
    public async Task<IActionResult> Index(int? categoryId, decimal? minPrice, decimal? maxPrice, string? search)
    {
        var query = _db.Games
            .Include(g => g.Categories)
            .AsNoTracking()
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(g => g.Categories.Any(c => c.Id == categoryId.Value));

        if (minPrice.HasValue)
            query = query.Where(g => g.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(g => g.Price <= maxPrice.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(g => g.Name.ToLower().Contains(search.ToLower()));

        var local_games = await query.OrderBy(g => g.Id).ToListAsync();

        // (optionnel) pour remplir une dropdown de catégories
        ViewBag.Categories = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();

        return View(local_games);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Game game)
    {
        if (!ModelState.IsValid) return View(game);
        _db.Games.Add(game);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var local_game = await _db.Games.FindAsync(id);
        if (local_game == null) return NotFound();
        return View(local_game);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Game game)
    {
    if (id != game.Id) return BadRequest();
    if (!ModelState.IsValid) return View(game);

    var local_game = await _db.Games.FirstOrDefaultAsync(g => g.Id == id);
    if (local_game == null) return NotFound();

    local_game.Name = game.Name;
    local_game.Price = game.Price;
    local_game.CurrentVersion = game.CurrentVersion;

    await _db.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var local_game = await _db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
        if (local_game == null) return NotFound();
        return View(local_game);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var local_game = await _db.Games.FindAsync(id);
        if (local_game == null) return NotFound();

        _db.Games.Remove(local_game);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
