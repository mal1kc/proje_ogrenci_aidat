using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;

void RegisterServices(IServiceCollection services, IConfiguration configuration)
{
    _ = services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.Name = Constants.AuthenticationCookieName;
            options.LoginPath = Constants.AuthenticationLoginPath;
        });
    _ = services.AddHttpContextAccessor();
    _ = services.AddDbContext<AppDbContext>();
    _ = services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromDays(27);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = Constants.CookieName;
    });
    _ = services.AddControllers();
    _ = services.AddEndpointsApiExplorer();
    _ = services.AddHttpContextAccessor();
}

async Task ConfigureAppAsync(WebApplication app)
{
    // stdout all of the configuration values of json file

    IConfiguration configuration = app.Configuration;
    // foreach (IConfigurationSection section in configuration.GetChildren())
    // {
    //     Console.WriteLine(section.Key);
    //     foreach (IConfigurationSection child in section.GetChildren())
    //     {
    //         Console.WriteLine($"\t{child.Key} : {child.Value}");
    //     }
    // }

    AppDbContext? ctx = app.Services.CreateScope().ServiceProvider.GetService<AppDbContext>();
    _ = ctx?.Database.EnsureCreated();

    if (!app.Environment.IsDevelopment())
    {
        _ = app.UseExceptionHandler("/Home/Error");
    }

    if (configuration.GetSection("SeedData").GetValue<bool>("SeedDB",true) == true)
    {
        Console.WriteLine("Seeding Database");

        // IDbSeeder<AppDbContext, SiteAdmin> siteAdminDbSeeder = new SiteAdminDBSeeder(context: ctx);

        List<IDbSeeder<AppDbContext>> DBseeders = new()
        {
            new SiteAdminDBSeeder(context: ctx, configuration: configuration)
        };
        if (configuration.GetSection("SeedData").GetValue<bool>("VerboseLogging", false))
            {

        Console.WriteLine("we have " + DBseeders.Count + " seeders");
            }


        foreach (var seeder in DBseeders)
        {
            if (configuration.GetSection("SeedData").GetValue<bool>("VerboseLogging", false))
            {
                Console.WriteLine("Seeding with " + seeder.GetType().Name);
            }
            await seeder.SeedAsync();
            await seeder.AfterSeedAsync();
        }



        Console.WriteLine("Database Seeded");

        _ = ctx.SaveChanges();
    }

    _ = app.UseHttpsRedirection();
    _ = app.UseHsts();
    _ = app.UseStaticFiles();
    _ = app.UseRouting();
    _ = app.UseSession();
    _ = app.UseAuthentication();
    _ = app.UseAuthorization();
    _ = app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
    _ = app.UseCors(builder => builder.AllowAnyOrigin());

}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// builder.Environment.ContentRootPath = Directory.GetCurrentDirectory() + "wwwroot";
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);


builder.Services.AddControllersWithViews();

RegisterServices(builder.Services, builder.Configuration);
WebApplication app = builder.Build();
ConfigureAppAsync(app);

app.Run();
