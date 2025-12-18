using Gauniv.WebServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Gauniv.WebServer.Api
{
    [ApiController]
    [Route("api/1.0.0/connected-user")]
    [Authorize]
    public class ConnectedUserController : ControllerBase
    {
        private readonly UserManager<User> userManager;

        public ConnectedUserController(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        // GET /api/1.0.0/connected-user/
        [HttpGet]
        public async Task<IActionResult> GetMe()
        {
            var local_user = await userManager.GetUserAsync(User);
            if (local_user == null) return Unauthorized();

            return Ok(new
            {
                local_user.Id,
                local_user.UserName,
                local_user.Email,
                local_user.FirstName,
                local_user.LastName
            });
        }
    }
}
