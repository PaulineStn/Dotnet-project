using Gauniv.WebServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Gauniv.WebServer.Models;
using X.PagedList;

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
    public async Task<IActionResult> Index(
        int? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? search,
        string? sort,
        int page = 1,
        int pageSize = 10)
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

        query = sort switch
        {
            "price_asc"  => query.OrderBy(g => g.Price).ThenBy(g => g.Id),
            "price_desc" => query.OrderByDescending(g => g.Price).ThenBy(g => g.Id),
            _            => query.OrderBy(g => g.Id)
        };


        ViewBag.Categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.CurrentCategoryId = categoryId;
        ViewBag.CurrentMinPrice = minPrice;
        ViewBag.CurrentMaxPrice = maxPrice;
        ViewBag.CurrentSort = sort;
        ViewBag.CurrentSearch = search;

        // pagination
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

        var pagedGames = new StaticPagedList<Game>(items, page, pageSize, totalCount);

        return View(pagedGames);
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
        
        // Stockage du fichier sur disque (streaming)
        var basePath = Path.Combine(
            _storageOptions.Value.GamesPath,
            game.Id.ToString());

        Directory.CreateDirectory(basePath);

        var safeGameName = game.Name.Trim();

        foreach (var c in Path.GetInvalidFileNameChars())
        {
            safeGameName = safeGameName.Replace(c, '_');
        }

        var safeFileName = $"{safeGameName}_{game.CurrentVersion}.exe";

        // Nom de fichier maîtrisé
        //var safeFileName = $"game_{game.Id}_{game.CurrentVersion}.bin";
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

        // Mise à jour du chemin
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

        var local_game = await _db.Games
            .Include(g => g.Categories)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (local_game == null) return NotFound();

        var local_categories = categoryIds?.Length > 0
            ? await _db.Categories.Where(c => categoryIds.Contains(c.Id)).ToListAsync()
            : new List<Category>();

        if (local_categories.Count == 0)
            ModelState.AddModelError("categoryIds", "Choisissez au moins une catégorie.");

        // Payload: optionnel -> on valide seulement si un fichier est fourni mais invalide
        if (payloadFile != null && payloadFile.Length == 0)
            ModelState.AddModelError("payloadFile", "Le fichier payload est vide.");

        if (!ModelState.IsValid)
        {
            // renvoie un modèle avec les catégories cochées
            game.Categories = local_categories;
            await PopulateCategoriesAsync();
            return View(game);
        }

        // Champs simples
        local_game.Name = game.Name;
        local_game.Price = game.Price;
        local_game.CurrentVersion = game.CurrentVersion;
        local_game.Description = game.Description;


        // Catégories
        local_game.Categories.Clear();
        foreach (var c in local_categories)
            local_game.Categories.Add(c);

        // ---- NOUVEAU: si l'admin a uploadé un payload, on le remplace
        if (payloadFile != null && payloadFile.Length > 0)
        {
            var basePath = Path.Combine(
                _storageOptions.Value.GamesPath,
                local_game.Id.ToString());

            Directory.CreateDirectory(basePath);

            var safeGameName = local_game.Name.Trim();

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                safeGameName = safeGameName.Replace(c, '_');
            }

            var safeFileName = $"{safeGameName}_{local_game.CurrentVersion}.exe";

            //var safeFileName = $"game_{local_game.Id}_{local_game.CurrentVersion}.bin";

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

            local_game.FilePath = filePath;

            // if (!string.IsNullOrWhiteSpace(oldPath) && System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
        }

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

        return View(local_games);
    }

    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<IActionResult> Checkout(int gameId)
    {
        var game = await _db.Games
            .Include(g => g.Categories)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return NotFound();

        // pourr empecher l'achat si déjà possédé
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var alreadyOwned = await _db.UserGamePurchases
            .AnyAsync(p => p.UserId == userId && p.GameId == gameId);

        ViewBag.AlreadyOwned = alreadyOwned;

        return View(game); // -> Views/Games/Checkout.cshtml
    }

    [Authorize(Roles = "User")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckoutConfirm(int gameId, string cardNumber)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var gameExists = await _db.Games.AnyAsync(g => g.Id == gameId);
        if (!gameExists) return NotFound();

        var alreadyOwned = await _db.UserGamePurchases
            .AnyAsync(p => p.UserId == userId && p.GameId == gameId);

        if (!alreadyOwned)
        {
            _db.UserGamePurchases.Add(new UserGamePurchase
            {
                UserId = userId,
                GameId = gameId,
                PurchasedAt = DateTime.UtcNow // si ta colonne existe, sinon enlève
            });

            await _db.SaveChangesAsync();
        }

        // Redirection vers Mes jeux
        return RedirectToAction(nameof(MyGames));
    }

    [HttpGet]
    public async Task<IActionResult> GamesStats()
    {
        // Total jeux
        var totalGames = await _db.Games.AsNoTracking().CountAsync();

        // Jeux par catégorie (y compris catégories à 0 jeu)
        var gamesByCategory = await _db.Categories
            .AsNoTracking()
            .Select(c => new GameStatsViewModel.CategoryCount
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                GameCount = c.Games.Count() // nécessite la nav Category.Games
            })
            .OrderByDescending(x => x.GameCount)
            .ThenBy(x => x.CategoryName)
            .ToListAsync();

        //TO DO
        // Moyenne de jeux "joués" par compte => ici: jeux achetés par utilisateur
        // - total d'utilisateurs
        // - total d'achats (ou distinct par user si tu veux éviter doublons)
        var usersCount = await _db.Users.AsNoTracking().CountAsync();

        var totalOwnedDistinct = await _db.UserGamePurchases
            .AsNoTracking()
            .Select(p => new { p.UserId, p.GameId })
            .Distinct()
            .CountAsync();

        decimal avg = 0m;
        if (usersCount > 0)
            avg = (decimal)totalOwnedDistinct / usersCount;

        var vm = new GameStatsViewModel
        {
            TotalGames = totalGames,
            GamesByCategory = gamesByCategory,
            AverageGamesPerAccount = Math.Round(avg, 2)
        };

        return View(vm); // Views/Games/Stats.cshtml
    }



}
