using OgrenciAidatSistemi.Models;
using Microsoft.EntityFrameworkCore;
namespace OgrenciAidatSistemi.Data{

    public class SiteAdminDBSeeder : DbSeeder<AppDbContext,SiteAdmin>
    {

        public SiteAdminDBSeeder(AppDbContext context,IConfiguration configuration) : base(context,configuration)
        {
            _seedData =                new()
                {
                    new(
                        username: "mal1kc",
                        firstName: "mal1kc",
                        lastName: "",
                        emailAddress: "admin@example.com",
                        passwordHash: SiteAdmin.ComputeHash("Aadmin123")
                    )
                };
           if (_verboselogging)
            {
                Console.WriteLine("created SiteAdminDBSeeder");
            }
        }


        protected override async Task SeedDataAsync()
        {
            if (_verboselogging)
            {
                Console.WriteLine("SiteAdminDBSeeder: Seeding SiteAdmins");
            }

            foreach (var siteadmin in _seedData)
            {
                if (_verboselogging)
                {
                    Console.WriteLine($"SiteAdminDBSeeder: Seeding SiteAdmins {siteadmin.Username}");
                }

                if (await _context.SiteAdmins.AnyAsync(a => a.Username == siteadmin.Username))
                {
                    continue;
                }
                siteadmin.CreatedAt = DateTime.Now;
                siteadmin.UpdatedAt = DateTime.Now;
                object value = await _context.SiteAdmins.AddAsync(siteadmin);
                if (_verboselogging)
                {
                    Console.WriteLine($"SiteAdminDBSeeder: Seeding SiteAdmins {siteadmin.Username} {value}");
                }
            }

            await _context.SaveChangesAsync();
        }

        protected override async Task AfterSeedDataAsync()
        {
            // search seed data in dbcontext if not raise exception
            if (_verboselogging)
            {
                Console.WriteLine("SiteAdminDBSeeder: AfterSeedDataAsync");
                Console.WriteLine(" we have seed data");
                for (int i = 0; i < _seedData.Count; i++)
                {
                    Console.WriteLine($"SiteAdminDBSeeder: AfterSeedDataAsync {_seedData[i].Username}");
                }
            }

            foreach (var siteadmin in _seedData)
            {
                if (_verboselogging)
                {
                    Console.WriteLine($"SiteAdminDBSeeder: AfterSeedDataAsync {siteadmin.Username}");
                }

                if (await _context.SiteAdmins.AnyAsync(a => a.Username == siteadmin.Username))
                {
                    continue;
                }
                throw new Exception($"SiteAdminDBSeeder: AfterSeedDataAsync {siteadmin.Username} not found in db");
            }
        }

        protected override async Task SeedRandomDataAsync()
        {
            throw new NotImplementedException();
        }}
    }
