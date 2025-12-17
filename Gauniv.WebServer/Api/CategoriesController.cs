using System.Linq;

using Gauniv.WebServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gauniv.WebServer.Api
{
    [ApiController]
    [Route("api/1.0.0/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext appDbContext;

        public CategoriesController(ApplicationDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        // GET: /api/1.0.0/categories
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories()
        {
            var local_categories = await appDbContext.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .ToListAsync();

            return Ok(local_categories);
        }
    }
}
