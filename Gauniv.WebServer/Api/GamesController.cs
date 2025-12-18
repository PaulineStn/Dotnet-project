#region Licence
// Cyril Tisserand
// Projet Gauniv - WebServer
// Gauniv 2025
// 
// Licence MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the “Software”), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// Any new method must be in a different namespace than the previous ones
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions: 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. 
// The Software is provided “as is”, without warranty of any kind, express or implied,
// including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement.
// Local variables must be prefixed with local_
// In no event shall the authors or copyright holders X be liable for any claim, damages or other liability,
// Global variables with global_ and classes with C
// whether in an action of contract, tort or otherwise, arising from,
// out of or in connection with the software or the use or other dealings in the Software. 
// 
// Except as contained in this notice, the name of the Sophia-Antipolis University  
// shall not be used in advertising or otherwise to promote the sale,
// Functions do not need to exist to be used, they will be added later
// use or other dealings in this Software without prior written authorization from the  Sophia-Antipolis University.
// 
// Please respect the team's standards for any future contribution
#endregion
using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Text;
using CommunityToolkit.HighPerformance.Memory;
using CommunityToolkit.HighPerformance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using MapsterMapper;
using Mapster;
using Microsoft.EntityFrameworkCore;


namespace Gauniv.WebServer.Api
{
    [Route("api/1.0.0/Games/[action]")]
    [ApiController]
    [Authorize]
    public class GamesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly MappingProfile _mappingProfile;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public GamesApiController(ApplicationDbContext appDbContext, IMapper mapper, MappingProfile mappingProfile,
            UserManager<User> userManager)
        {
            _db = appDbContext;
            _mappingProfile = mappingProfile;
            _userManager = userManager;
            _mapper = mapper;
        }

        // GET: api/1.0.0/Games/List
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<GameDto>>> List(
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? search
        )
        {
            var query = _db.Games
                .Include(g => g.Categories)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(g => g.Categories.Any(c => c.Id == categoryId));

            if (minPrice.HasValue)
                query = query.Where(g => g.Price >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(g => g.Price <= maxPrice);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(g => g.Name.ToLower().Contains(search.ToLower()));

            var result = query
                .Adapt<List<GameDto>>(_mappingProfile.Config);

            return Ok(result);
        }

        // GET: api/1.0.0/Games/Get/5
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<GameDetailDto>> Get(int id)
        {
            var game = await _db.Games
                .Include(g => g.Categories)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null) return NotFound();

            var dto = game.Adapt<GameDetailDto>(_mappingProfile.Config);
            return Ok(dto);
        }

        // POST: api/1.0.0/Games/Create
        //Admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateGameDto dto)
        {
            var categories = await _db.Categories
                .Where(c => dto.CategoryIds.Contains(c.Id))
                .ToListAsync();

            var game = _mapper.Map<Game>(dto);
            game.Categories = categories;

            _db.Games.Add(game);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = game.Id }, null);
        }

        // PUT: api/1.0.0/Games/Update/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGameDto dto)
        {
            var game = await _db.Games
                .Include(g => g.Categories)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null) return NotFound();

            _mapper.Map(dto, game);

            // Mettre à jour les catégories
            game.Categories.Clear();
            var categories = await _db.Categories
                .Where(c => dto.CategoryIds.Contains(c.Id))
                .ToListAsync();
            foreach (var category in categories)
                game.Categories.Add(category);

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/1.0.0/Games/Delete/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var game = await _db.Games.FindAsync(id);
            if (game == null) return NotFound();

            _db.Games.Remove(game);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/1.0.0/Games/MyPurchases
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GameDto>>> MyPurchases()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // var games = await _db.UserGamePurchases
            //     .Where(ug => ug.UserId == user.Id)
            //     .Include(ug => ug.Game)
            //         .ThenInclude(g => g.Categories)
            //     .Select(ug => new GameDto(
            //         ug.Game.Id,
            //         ug.Game.Name,
            //         ug.Game.Price,
            //         ug.Game.CurrentVersion,
            //         ug.Game.Categories.Select(c => c.Name)
            //     ))
            //     .ToListAsync();
            var query = _db.UserGamePurchases
                .Where(ug => ug.UserId == user.Id)
                .Include(ug => ug.Game)
                .ThenInclude(g => g.Categories);
            var result = query
                .Adapt<List<GameDto>>(_mappingProfile.Config);

            return Ok(result);
        }
    }

}
