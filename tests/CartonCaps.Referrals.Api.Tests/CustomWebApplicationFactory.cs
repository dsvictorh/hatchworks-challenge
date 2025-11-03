using CartonCaps.Referrals.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace CartonCaps.Referrals.Api.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(config =>
        {
            var dict = new Dictionary<string, string?>
            {
                {
                    "ConnectionStrings:Postgres",
                    "Host=localhost;Port=5432;Database=cartoncaps_test;Username=postgres;Password=postgres"
                }
            };
            config.AddInMemoryCollection(dict!);
        });
        builder.ConfigureServices(services =>
        {
            // Ensure test database exists
            using (var cn = new NpgsqlConnection(
                       "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres"))
            {
                cn.Open();
                using var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = 'cartoncaps_test'";
                var exists = cmd.ExecuteScalar() != null;
                if (!exists)
                {
                    using var create = cn.CreateCommand();
                    create.CommandText = "CREATE DATABASE cartoncaps_test";
                    create.ExecuteNonQuery();
                }
            }

            // Ensure database is migrated and seeded for tests
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ReferralsDbContext>();
            db.Database.Migrate();
            db.ReferralEvents.RemoveRange(db.ReferralEvents);
            db.ReferralSessions.RemoveRange(db.ReferralSessions);
            db.ReferralLinks.RemoveRange(db.ReferralLinks);
            db.Referrals.RemoveRange(db.Referrals);
            db.ReferralCodes.RemoveRange(db.ReferralCodes);
            db.SaveChanges();
            DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
        });
    }
}
