using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HotTubShop.Web.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<IdentitySeedOptions>>().Value;

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (string.IsNullOrWhiteSpace(options.Email))
        {
            return;
        }

        var user = await userManager.FindByEmailAsync(options.Email);
        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(options.Password))
            {
                return;
            }

            user = new IdentityUser
            {
                UserName = options.Email,
                Email = options.Email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, options.Password);
            if (!createResult.Succeeded)
            {
                return;
            }
        }
        else
        {
            // Keep seeded admin account operational by aligning email confirmation and optional configured password.
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await userManager.UpdateAsync(user);
            }

            if (!string.IsNullOrWhiteSpace(options.Password))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await userManager.ResetPasswordAsync(user, token, options.Password);
                if (!resetResult.Succeeded)
                {
                    return;
                }
            }
        }

        if (!await userManager.IsInRoleAsync(user, "Admin"))
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}
