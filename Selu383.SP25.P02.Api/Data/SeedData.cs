using Microsoft.AspNetCore.Identity;
using Selu383.SP25.P02.Api.Features.Theaters;
using Selu383.SP25.P02.Api.Features.Users;

namespace Selu383.SP25.P02.Api.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<DataContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            // Seed Roles
            if (!context.Roles.Any())
            {
                await roleManager.CreateAsync(new Role { Name = "Admin" });
                await roleManager.CreateAsync(new Role { Name = "User" });
            }

            // Seed Users
            if (!context.Users.Any())
            {
                // Create Admin
                var adminUser = new User { UserName = "galkadi" };
                await userManager.CreateAsync(adminUser, "Password123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");

                // Create Regular Users
                var bobUser = new User { UserName = "bob" };
                await userManager.CreateAsync(bobUser, "Password123!");
                await userManager.AddToRoleAsync(bobUser, "User");

                var sueUser = new User { UserName = "sue" };
                await userManager.CreateAsync(sueUser, "Password123!");
                await userManager.AddToRoleAsync(sueUser, "User");
            }

            // Seed Theaters
            if (!context.Theaters.Any())
            {
                context.Theaters.AddRange(
                    new Theater
                    {
                        Name = "AmStar Cinema Hammond",
                        Address = "1000 CM Fagan Dr, Hammond, LA 70403",
                        SeatCount = 200
                    },
                    new Theater
                    {
                        Name = "Celebrity Theatres Hammond",
                        Address = "1818 S Morrison Blvd, Hammond, LA 70403",
                        SeatCount = 150
                    },
                    new Theater
                    {
                        Name = "Columbia Theatre",
                        Address = "220 E Thomas St, Hammond, LA 70401",
                        SeatCount = 850
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}

