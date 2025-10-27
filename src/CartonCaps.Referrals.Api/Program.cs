using System.Reflection;
using CartonCaps.Referrals.Core.Application.Abstractions;
using CartonCaps.Referrals.Core.Application.Contracts;
using CartonCaps.Referrals.Core.Domain.ValueObjects;
using CartonCaps.Referrals.Infrastructure.Persistence;
using CartonCaps.Referrals.Infrastructure.Services;
using Microsoft.OpenApi.Models;
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
            var builder = WebApplication.CreateBuilder(args);

            // Add Serilog to the dependency injection container
            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));

            // Services
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Carton Caps Referrals API",
                    Description = "Share your referral codes and earn rewards when friends join!",
                    Contact = new OpenApiContact
                    {
                        Name = "Victor Herrera",
                        Email = "vherera-jobs@gmail.com"
                    }
                });

                // Include XML comments for better documentation
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
                // Include XML comments from Core project (contracts, models)
                var coreXmlFilename = "CartonCaps.Referrals.Core.xml";
                var coreXmlPath = Path.Combine(AppContext.BaseDirectory, coreXmlFilename);
                if (File.Exists(coreXmlPath)) options.IncludeXmlComments(coreXmlPath);
            });

            builder.Services.AddSingleton<InMemoryDb>();
            builder.Services.AddSingleton<IReferralService, ReferralService>();

            var app = builder.Build();

            // Load some test data
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<InMemoryDb>();
                SeedData.Load(db);
            }

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Carton Caps Referrals API v1");
                options.DocumentTitle = "Carton Caps Referrals API v1";
                //options.RoutePrefix = string.Empty; // Serve Swagger UI at "/" for demo purposes
            });

            app.MapGet("/api/v1/referrals", (HttpContext ctx, IReferralService svc) =>
                {
                    var userId = "usr_referrer_001"; // simulates authenticated user

                    try
                    {
                        var result = svc.GetMyReferrals(userId);
                        return Results.Ok(result);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("Error getting referrals: {Error}", ex.Message);
                        throw;
                    }
                })
                .WithName("GetReferrals")
                .WithSummary("Get referrals")
                .WithTags("Referrals");

            app.MapPost("/api/v1/referrals/link", (IReferralService svc, GenerateLinkRequest req) =>
                {
                    var userId = "usr_referrer_001";

                    try
                    {
                        var res = svc.GenerateLink(userId, req.Channel);
                        return Results.Created("/api/v1/referrals/link", res);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed creating link");
                        throw;
                    }
                })
                .WithName("GenerateReferralLink")
                .WithSummary("Create referral link")
                .WithTags("Referrals");

            app.MapPost("/api/v1/referrals/share-message", (IReferralService svc, ShareMessageRequest req) =>
                {
                    var userId = "usr_referrer_001";

                    try
                    {
                        var res = svc.GetShareMessage(userId, req.Channel, req.Locale);
                        return Results.Ok(res);
                    }
                    catch (Exception ex)
                    {
                        // Just log and rethrow - maybe template service is down
                        Log.Fatal(ex, "Message template failed");
                        throw;
                    }
                })
                .WithName("GetShareMessage")
                .WithSummary("Get share message")
                .WithTags("Referrals");

            app.MapPost("/api/v1/referrals/share", (IReferralService svc, ShareEventRequest req) =>
                {
                    var userId = "usr_referrer_001";

                    try
                    {
                        svc.RecordShare(userId, req);
                        return Results.Accepted();
                    }
                    catch (Exception ex)
                    {
                        Log.Debug("Share event recording issue: {Message}", ex.Message);
                        throw;
                    }
                })
                .WithName("RecordShareEvent")
                .WithSummary("Track share")
                .WithTags("Analytics");

            app.MapPost("/api/v1/referrals/events", (ReferralEventIn req) =>
                {
                    // Event processing not implemented
                    return Results.Accepted();
                })
                .WithName("ProcessReferralEvent")
                .WithSummary("Process events")
                .WithTags("Analytics");

            app.MapPost("/api/v1/referrals/verify", (IReferralService svc, VerifyReferralRequest req) =>
                {
                    var res = svc.Verify(req);
                    return Results.Ok(res);
                })
                .WithName("VerifyReferralCode")
                .WithSummary("Verify code")
                .WithTags("Referrals");

            app.MapPost("/api/v1/referrals/session", (IReferralService svc, CreateSessionRequest req) =>
                {
                    try
                    {
                        var res = svc.CreateSession(req);
                        return Results.Created($"/api/v1/referrals/session/{res.SessionId}", res);
                    }
                    catch (Exception ex)
                    {
                        Log.Information("Session endpoint error: {Exception}", ex);
                        throw;
                    }
                })
                .WithName("CreateReferralSession")
                .WithSummary("Create session")
                .WithTags("Sessions");

            app.MapPatch("/api/v1/referrals/redeem", (IReferralService svc, RedeemReferralRequest req) =>
                {
                    var userId = "usr_referrer_001";

                    try
                    {
                        var res = svc.Redeem(userId, req);
                        if (res.RewardEligible) Log.Information("Referral success! {UserId} earned a reward", userId);
                        return Results.Ok(res);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to redeem referral for user {UserId}", userId);
                        throw;
                    }
                })
                .WithName("RedeemReferral")
                .WithSummary("Redeem referral")
                .WithTags("Referrals");

            Log.Information("API started! Check /swagger for docs");

            app.Run();
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

    public class ReferralEventIn
    {
        public string Event { get; set; } = string.Empty;
        public string ReferralCode { get; set; } = string.Empty;
        public string? EventId { get; set; }
        public DeviceInfo? DeviceInfo { get; set; }
    }
}
