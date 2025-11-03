using Serilog;

namespace CartonCaps.Referrals.Api;

public class Program
{
    public static void Main(string[] args)
    {
        // Configure Serilog for logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/cartoncaps-referrals-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10_000_000,
                rollOnFileSizeLimit: true)
            .Enrich.WithProperty("Application", "CartonCaps.Referrals.Api")
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting API...");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "API crashed on startup");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // Enable WebApplicationFactory to build the host in tests
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration))
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
