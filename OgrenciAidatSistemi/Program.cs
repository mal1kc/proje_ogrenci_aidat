using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore; // required for UseSqlServer
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
            // **Transient**: A new instance is created every time a service is requested. This is useful for lightweight, stateless services.

            //  **Scoped**: A new instance is created once per scope. A scope is created on each request in an ASP.NET Core application, so you can think of scoped lifetime as per request.

            //  **Singleton**: A single instance is created and that same instance is used every time the service is requested.

            _ = services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/Login";
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
            _ = services.AddDbContext<AppDbContext>(
#if DEBUG
#else
                options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                )
#endif
            );
            _ = services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(27);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = Constants.CookieName;
            });
            _ = services.AddControllers();
            // for https
            _ = services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            _ = services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });
            _ = services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = 8989;
            });
            // end for https
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
            // _ = services.AddTransient<PaymentCreatorInvocable>();
            _ = services.AddScoped<PaymentCreatorInvocable>();
            _ = services.AddScheduler();
        }

        async Task ConfigureAppAsync(WebApplication app)
        {
            IConfiguration configuration = app.Configuration;

            // write connection string to console
#if DEBUG
#else
            Console.WriteLine(
                "Connection String: " + configuration.GetConnectionString("DefaultConnection")
            );
#endif

            AppDbContext? ctx = app
                .Services.CreateScope()
                .ServiceProvider.GetService<AppDbContext>();
            _ = ctx?.Database.EnsureCreated();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                _ = app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }
#if DEBUG
            Console.WriteLine("Using development error handler");
            _ = app.UseDeveloperExceptionPage();
#endif
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

                    _ = ctx.SaveChanges();
                }
                Console.WriteLine("Database Seeded");

                var provider = app
                    .Services.UseScheduler(scheduler =>
                    {
                        scheduler
                            .Schedule<PaymentCreatorInvocable>()
#if DEBUG
                            .EveryThirtyMinutes()
                            // .RunOnceAtStart()
#else
                            .Daily()
#endif
                            .PreventOverlapping("PaymentCreator");
                    })
                    .LogScheduledTaskProgress(app.Services.GetService<ILogger<IScheduler>>());
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

            // for https
            _ = app.UseForwardedHeaders(
                new ForwardedHeadersOptions
                {
                    ForwardedHeaders =
                        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                }
            );
            // end for https
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
        if (
            OgrenciAidatSistemi.Models.Extensions.PaymentMethodSpecificFields.ValidateFields()
            == false
        )
        {
            throw new Exception("PaymentMethodSpecificFields is not valid");
        }

        #endregion

        await app.RunAsync();
    }
}
