#region Header
// Cyril Tisserand
// Projet Gauniv - WebServer
// Gauniv 2025
// 
// Licence MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// Any new method must be in a different namespace than the previous ones
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions: 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. 
// The Software is provided "as is", without warranty of any kind, express or implied,
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
using Gauniv.WebServer.Websocket;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Text;

namespace Gauniv.WebServer.Services
{
    public class SetupService : IHostedService
    {
        private ApplicationDbContext? applicationDbContext;
        private readonly IServiceProvider serviceProvider;
        private Task? task;
        private readonly IOptions<StorageOptions> _storageOptions;

        public SetupService(IServiceProvider serviceProvider, IOptions<StorageOptions> storageOptions)
        {
            this.serviceProvider = serviceProvider;
            _storageOptions = storageOptions;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = serviceProvider.CreateScope()) // this will use `IServiceScopeFactory` internally
            {
                applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                // var signInManager = scope.ServiceProvider.GetService<SignInManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();


                if (applicationDbContext is null)
                {
                    throw new Exception("ApplicationDbContext is null");
                }
                
                if (applicationDbContext.Database.GetPendingMigrations().Any())
                {
                    applicationDbContext.Database.Migrate();
                }
                
                 // 1️⃣ Créer le rôle Admin s'il n'existe pas
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // 2️⃣ Créer un utilisateur simple
                var userEmail = "test@test.com";
                var user = await userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = userEmail,
                        Email = userEmail,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(user, "Password123!"); // mot de passe simple pour test
                }
                
                // 3️⃣ Créer un utilisateur Admin
                var adminEmail = "admin@test.com";
                var admin = await userManager.FindByEmailAsync(adminEmail);
                if (admin == null)
                {
                    admin = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(admin, "AdminPassword123!");
                }
                
                // 4️⃣ Assigner le rôle Admin
                if (!await userManager.IsInRoleAsync(admin, "Admin"))
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
                
                
                // Create Category
                if (!applicationDbContext.Categories.Any())
                {
                    applicationDbContext.Categories.AddRange(
                        new Category { Name = "Action" },
                        new Category { Name = "RPG" },
                        new Category { Name = "Indie" }
                    );
                    applicationDbContext.SaveChanges(); // pour avoir les Id
                }
                
                if (!applicationDbContext.Games.Any())
                {
                    var local_action = applicationDbContext.Categories.First(c => c.Name == "Action");
                    var local_rpg = applicationDbContext.Categories.First(c => c.Name == "RPG");

                    var local_game1 = new Game
                    {
                        Name = "Space Blaster",
                        Description = "Fast action shooter",
                        Price = 19.99m,
                        CurrentVersion = "1.0.0",
                        Categories = new List<Category> { local_action }
                    };

                    var local_game2 = new Game
                    {
                        Name = "Dungeon Quest",
                        Description = "Classic RPG adventure",
                        Price = 29.99m,
                        CurrentVersion = "0.9.1",
                        Categories = new List<Category> { local_rpg }
                    };
                    
                    var games = new[] { local_game1, local_game2 };

                    // 1️⃣ Ajouter + sauvegarder POUR AVOIR LES ID
                    applicationDbContext.Games.AddRange(games);
                    await applicationDbContext.SaveChangesAsync();
                    
                    Console.WriteLine($"games:  {games.Count()}");
                    foreach (var game in games)
                    {
                        var basePath = Path.Combine(_storageOptions.Value.GamesPath, game.Id.ToString());
                        Directory.CreateDirectory(basePath);
                        
                        // Console.WriteLine($"basePath:  {basePath}");


                        var fileName = $"game_{game.Id}_{game.CurrentVersion}.bin";
                        var filePath = Path.Combine(basePath, fileName);

                        await File.WriteAllBytesAsync(
                            filePath,
                            Encoding.UTF8.GetBytes($"FAKE_BINARY_{game.Name.ToUpperInvariant()}")
                        );

                        game.FilePath = filePath;
                    }
                    // 3️⃣ Sauvegarder FilePath
                    await applicationDbContext.SaveChangesAsync();
                }
                

                // 5️⃣ Sauvegarder les changements (au cas où)
                await applicationDbContext.SaveChangesAsync();
                
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
