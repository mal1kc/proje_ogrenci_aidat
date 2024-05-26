using Coravel;
using Microsoft.AspNetCore.Authentication.Cookies;
using NReco.Logging.File;
using OgrenciAidatSistemi.Configurations;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Data.DBSeeders;
using OgrenciAidatSistemi.Invokables;
using OgrenciAidatSistemi.Services;

internal class Program
{
    private static async Task Main(string[] args)
    {
        void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            _ = services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = Constants.AuthenticationCookieName;
                    options.AccessDeniedPath = Constants.AuthenticationAccessDeniedPath;
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToAccessDenied = context =>
                        {
                            context.Response.Redirect(Constants.AuthenticationAccessDeniedPath);
                            return Task.CompletedTask;
                        }
                    };
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
            _ = services.AddLogging();

            _ = services.AddLogging(loggingBuilder =>
            {
                var loggingSection = configuration.GetSection("Logging");
                loggingBuilder.AddFile(loggingSection);
            });

            _ = services.AddScoped<UserService>();
            _ = services.AddScoped<FileService>();
            _ = services.AddScoped<ReceiptService>();
            _ = services.AddScoped<StudentService>();
            _ = services.AddScoped<ExportService>();
            _ = services.AddScoped<PaymentService>();
            _ = services.AddScheduler();
        }

        async Task ConfigureAppAsync(WebApplication app)
        {
            IConfiguration configuration = app.Configuration;

            AppDbContext? ctx = app
                .Services.CreateScope()
                .ServiceProvider.GetService<AppDbContext>();
            _ = ctx?.Database.EnsureCreated();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                _ = app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }
            else
            {
                Console.WriteLine("Using development error handler");
                _ = app.UseDeveloperExceptionPage();
            }
            if (ctx == null)
            {
                throw new Exception("AppDbContext is null");
            }

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var studentService = services.GetRequiredService<StudentService>();

                if (configuration.GetSection("SeedData").GetValue("SeedSiteAdmin", true) == true)
                {
                    var siteAdminSeeder = new SiteAdminDBSeeder(
                        context: ctx,
                        configuration: configuration,
                        logger: logger
                    );

                    await siteAdminSeeder.SeedAsync();
                }

                if (configuration.GetSection("SeedData").GetValue("SeedDB", true) == true)
                {
                    var _verbs = configuration
                        .GetSection("SeedData")
                        .GetValue("VerboseLogging", false);
                    Console.WriteLine("Seeding Database");

                    List<IDbSeeder<AppDbContext>> DBseeders =
                    [
                        new SchoolAdminDBSeeder(
                            context: ctx,
                            configuration: configuration,
                            logger: logger,
                            maxSeedCount: 10,
                            randomSeed: true
                        ),
                        new SchoolDBSeeder(
                            context: ctx,
                            configuration: configuration,
                            logger: logger,
                            maxSeedCount: 10,
                            randomSeed: true
                        ),
                        new StudentDBSeeder(
                            context: ctx,
                            configuration: configuration,
                            logger: logger,
                            studentService: studentService,
                            maxSeedCount: 40,
                            randomSeed: true
                        ),
                        new PaymentDBSeeder(
                            context: ctx,
                            configuration: configuration,
                            logger: logger,
                            studentService: studentService,
                            randomSeed: true
                        )
                    ];
                    if (_verbs)
                    {
                        Console.WriteLine("we have " + DBseeders.Count + " seeders");
                    }

                    // seed random data for each seeder other than the SiteAdminDBSeeder
                    foreach (var seeder in DBseeders)
                    {
                        if (_verbs)
                        {
                            Console.WriteLine("Seeding with " + seeder.GetType().Name);
                        }
                        await seeder.SeedAsync();
                        await seeder.AfterSeedAsync();
                    }

                    // schedule payment service

                    _ = ctx.SaveChanges();
                }
                Console.WriteLine("Database Seeded");

                var provider = app.Services.UseScheduler(scheduler =>
                {
                    scheduler
                        .Schedule<PaymentCreatorInvokable>()
                        .Daily()
                        .RunOnceAtStart()
                        .PreventOverlapping("PaymentCreator");
                });
            }

            _ = app.UseHttpsRedirection();
            _ = app.UseHsts();
            _ = app.UseStaticFiles();
            _ = app.UseRouting();
            _ = app.UseSession();
            _ = app.UseAuthentication();
            _ = app.UseAuthorization();
            _ = app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );
            _ = app.UseCors(builder => builder.AllowAnyOrigin());

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapControllers();
            });
        }

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // builder.Environment.ContentRootPath = Directory.GetCurrentDirectory() + "wwwroot";
        builder.Configuration.AddJsonFile(
#if DEBUG
            "appsettings.Development.json",
#else
            "appsettings.json",
#endif
            optional: false,
            reloadOnChange: false
        );

        builder.Services.AddControllersWithViews();

        RegisterServices(builder.Services, builder.Configuration);
        WebApplication app = builder.Build();
        await ConfigureAppAsync(app);

        #region "some pre-run validations"
        if (OgrenciAidatSistemi.Models.PaymentMethodSpecificFields.ValidateFields() == false)
        {
            throw new Exception("PaymentMethodSpecificFields is not valid");
        }

        #endregion

        await app.RunAsync();
    }
}
