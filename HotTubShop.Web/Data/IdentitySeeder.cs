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
        user ??= await userManager.FindByNameAsync(options.Email);
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
                var errors = string.Join("; ", createResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Unable to create seeded admin user '{options.Email}'. {errors}");
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
                IdentityResult resetResult;
                if (await userManager.HasPasswordAsync(user))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    resetResult = await userManager.ResetPasswordAsync(user, token, options.Password);
                }
                else
                {
                    resetResult = await userManager.AddPasswordAsync(user, options.Password);
                }

                if (!resetResult.Succeeded)
                {
                    var errors = string.Join("; ", resetResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new InvalidOperationException($"Unable to set password for seeded admin user '{options.Email}'. {errors}");
                }
            }

            // If lockout was triggered by failed attempts, clear it for admin bootstrap.
            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.ResetAccessFailedCountAsync(user);
        }

        if (!await userManager.IsInRoleAsync(user, "Admin"))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, "Admin");
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join("; ", addRoleResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Unable to assign Admin role to '{options.Email}'. {errors}");
            }
        }
    }
}
