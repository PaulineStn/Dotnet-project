using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gauniv.WebServer.Controllers;

[ApiController]
[Route("api/1.0.0/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CategoriesController(ApplicationDbContext appDbContext)
    {
        _db = appDbContext;
    }

    // GET api/categories
    [HttpGet]
    [AllowAnonymous]
    public async Task<IEnumerable<CategoryDto>> GetAll()
    {
        return await _db.Categories
            .Select(c => new CategoryDto(c.Id, c.Name))
            .ToListAsync();
    }

    // GET api/categories/{id}
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> Get(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        return new CategoryDto(category.Id, category.Name);
    }

    // POST api/categories
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
    {
        var category = new Category { Name = dto.Name };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get),
            new { id = category.Id },
            new CategoryDto(category.Id, category.Name));
    }

    // PUT api/categories/{id}
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateCategoryDto dto)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        category.Name = dto.Name;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/categories/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        // Grâce au cascade delete, GameCategories est nettoyée automatiquement
        return NoContent();
    }
}
