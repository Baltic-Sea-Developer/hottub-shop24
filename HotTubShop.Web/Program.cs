using HotTubShop.Web.Data;
using HotTubShop.Web.Infrastructure;
using HotTubShop.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var appDataPath = ResolveAppDataPath(builder.Environment.ContentRootPath);
builder.Services.AddSingleton(new AppDataPathProvider { DataDirectory = appDataPath });

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IProductCatalogService, JsonProductCatalogService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={Path.Combine(appDataPath, "auth.db")}"));
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.Configure<IdentitySeedOptions>(builder.Configuration.GetSection("IdentitySeed"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.Use(async (context, next) =>
{
    if (context.Request.Path.Equals("/Identity/Account/Register", StringComparison.OrdinalIgnoreCase) ||
        context.Request.Path.Equals("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase))
    {
        if (context.Request.Query.TryGetValue("returnUrl", out var returnUrlValues))
        {
            var returnUrl = returnUrlValues.ToString();
            if (IsPrivilegedReturnUrl(returnUrl))
            {
                var target = context.Request.Path + "?returnUrl=%2F";
                context.Response.Redirect(target);
                return;
            }
        }
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

try
{
    await IdentitySeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Identity seeding failed during startup. App will continue running.");
}

try
{
    using var scope = app.Services.CreateScope();
    var catalog = scope.ServiceProvider.GetRequiredService<IProductCatalogService>();
    var products = await catalog.GetProductsAsync();
    var seededIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "arctic-zen", "fjord-lounge" };
    foreach (var p in products.Where(x => seededIds.Contains(x.Id)))
    {
        await catalog.DeleteProductAsync(p.Id);
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Catalog cleanup failed during startup. App will continue running.");
}

app.Run();

static string ResolveAppDataPath(string contentRootPath)
{
    var primary = Path.Combine(contentRootPath, "App_Data");
    try
    {
        Directory.CreateDirectory(primary);
        return primary;
    }
    catch
    {
        var fallback = Path.Combine(Path.GetTempPath(), "HotTubShop24", "App_Data");
        Directory.CreateDirectory(fallback);
        return fallback;
    }
}

static bool IsPrivilegedReturnUrl(string? returnUrl)
{
    if (string.IsNullOrWhiteSpace(returnUrl))
    {
        return false;
    }

    var normalized = Uri.UnescapeDataString(returnUrl.Trim());
    return normalized.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase);
}
