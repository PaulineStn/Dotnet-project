// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Security.Claims;
// using System.Threading.Tasks;

// namespace Gauniv.WebServer.Data
// {
//     public static class DbInitializer 
//     {
//         public static async Task InitializeAsync(ApplicationDbContext context, UserManager<User> userManager)
//         {
//             context.Database.EnsureCreated();

//             // Look for any users.
//             if (context.Users.Any())
//             {
//                 return;   // DB has been seeded
//             }
//             var users = new User[]
//             {
//                 new User{ FirstName="Cyril", LastName="Tisserand", UserName="   "},
//                 new User{ FirstName="John", LastName="Doe", UserName="johndoe"},
//                 new User{ FirstName="Jane", LastName="Smith", UserName="janesmith"},
//             };
//             if (context.Games.Any())
//             {
//                 return;   // DB has been seeded
//             }
//             var games = new Game[]
//             {
//                 new Game{ Name="The Witcher 3: Wild Hunt", Description="An action role-playing game developed by CD Projekt Red.", Price=29.99M},
//                 new Game{ Name="Cyberpunk 2077", Description="An open-world, action-adventure story set in Night City.", Price=59.99M},
//                 new Game{ Name="Red Dead Redemption 2", Description="An epic tale of life in America at the dawn of the modern age.", Price=59.99M},
//                 new Game{ Name="God of War", Description="A mythological adventure game developed by Santa Monica Studio.", Price=49.99M},
//                 new Game{ Name="Hades", Description="A rogue-like dungeon crawler developed by Supergiant Games.", Price=24.99M}
//             };  

//             var roles = new IdentityRole[] {
//                 new IdentityRole("basic"),
//                 new IdentityRole("advance"),
//                 new IdentityRole("admin")

//             };
//             foreach(var role in roles)
//             {
//                 roleManager.CreateAsync(role).Wait();
//             }

//             var user = new IdentityUser()
//            {
//                 Id= "test@dot.net",
//                 EmailConfirmed = true,
//                 NormalizedUserName = "test@dot.net",
//                 Email = "test@dot.net",
//                 UserName = "test@dot.net",
//                 NormalizedEmail = "test@dot.net"
//             };

//             userManager.CreateAsync(user, "Test1A$").Wait();
//             userManager.AddToRoleAsync(user, roles[0].Name).Wait();
//         }
//     }
                
// }