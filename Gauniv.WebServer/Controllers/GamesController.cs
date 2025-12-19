using Gauniv.WebServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
namespace Gauniv.WebServer.Controllers;

public class GamesController : Controller
{
    private readonly ApplicationDbContext _db;
    public GamesController(ApplicationDbContext db, IOptions<StorageOptions> storageOptions) {
        _db = db;
        _storageOptions = storageOptions;
    }
    private readonly IOptions<StorageOptions> _storageOptions;


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
    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new Game());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Game game, int[]? categoryIds, IFormFile? payloadFile)
    {
        var local_categories = categoryIds?.Length > 0
            ? await _db.Categories.Where(c => categoryIds.Contains(c.Id)).ToListAsync()
            : new List<Category>();

        if (local_categories.Count == 0)
            ModelState.AddModelError("Categories", "Choisissez au moins une catégorie.");

        if (payloadFile == null || payloadFile.Length == 0)
            ModelState.AddModelError(nameof(payloadFile), "Veuillez sélectionner un fichier payload.");
        

        if (!ModelState.IsValid)
        {
            game.Categories = local_categories;
            await PopulateCategoriesAsync();
            return View(game);
        }
        
        game.Categories = local_categories;
        _db.Games.Add(game);
        await _db.SaveChangesAsync();
        
        // 2️⃣ Stockage du fichier sur disque (streaming)
        var basePath = Path.Combine(
            _storageOptions.Value.GamesPath,
            game.Id.ToString());

        Directory.CreateDirectory(basePath);

        // Nom de fichier maîtrisé
        var safeFileName = $"game_{game.Id}_{game.CurrentVersion}.bin";
        var filePath = Path.Combine(basePath, safeFileName);

        await using (var fileStream = new FileStream(
                         filePath,
                         FileMode.Create,
                         FileAccess.Write,
                         FileShare.None,
                         bufferSize: 81920,
                         useAsync: true))
        {
            await payloadFile.CopyToAsync(fileStream);
        }

        // 3️⃣ Mise à jour du chemin
        game.FilePath = filePath;
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var local_game = await _db.Games
            .Include(g => g.Categories)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (local_game == null) return NotFound();
        await PopulateCategoriesAsync();
        return View(local_game);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Game game, int[]? categoryIds, IFormFile? payloadFile)
    {
    if (id != game.Id) return BadRequest();
    //if (!ModelState.IsValid) return View(game);

    var local_game = await _db.Games
        .Include(g => g.Categories)
        .FirstOrDefaultAsync(g => g.Id == id);

    if (local_game == null) return NotFound();

    var local_categories = categoryIds?.Length > 0
        ? await _db.Categories.Where(c => categoryIds.Contains(c.Id)).ToListAsync()
        : new List<Category>();

    if (local_categories.Count == 0)
        ModelState.AddModelError("categoryIds", "Choisissez au moins une catégorie.");

    if (!ModelState.IsValid)
    {
        game.Categories = local_categories;
        await PopulateCategoriesAsync();
        return View(game);
    }

    local_game.Name = game.Name;
    local_game.Price = game.Price;
    local_game.CurrentVersion = game.CurrentVersion;

    local_game.Categories.Clear();
    foreach (var c in local_categories)
        local_game.Categories.Add(c);

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

    private async Task PopulateCategoriesAsync()
    {
        ViewBag.Categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    [Authorize(Roles = "User")]
    public async Task<IActionResult> MyGames()
    {
        var local_userId  = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var local_games = await _db.UserGamePurchases
            .Where(p => p.UserId == local_userId)
            .Include(p => p.Game)
                .ThenInclude(g => g.Categories)
            .Select(p => p.Game)
            .AsNoTracking()
            .ToListAsync();

        return View("Index", local_games);
    }

}
