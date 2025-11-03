using System.Reflection;
using System.Threading.RateLimiting;
using CartonCaps.Referrals.Api.Endpoints;
using CartonCaps.Referrals.Api.Exceptions;
using CartonCaps.Referrals.Core.Application.Abstractions;
using CartonCaps.Referrals.Infrastructure.Exceptions;
using CartonCaps.Referrals.Infrastructure.Persistence;
using CartonCaps.Referrals.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using FV = FluentValidation;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace CartonCaps.Referrals.Api;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddHealthChecks();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Carton Caps Referrals API",
                Description = "Share your referral codes and earn rewards when friends join!",
                Contact = new OpenApiContact { Name = "Victor Herrera", Email = "vherera-jobs@gmail.com" }
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
            var coreXmlFilename = "CartonCaps.Referrals.Core.xml";
            var coreXmlPath = Path.Combine(AppContext.BaseDirectory, coreXmlFilename);
            if (File.Exists(coreXmlPath)) options.IncludeXmlComments(coreXmlPath);
        });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("linkPolicy", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "anon",
                key => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 3,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
            options.AddPolicy("sharePolicy", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "anon",
                key => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
        });

        var cs = _configuration.GetConnectionString("Postgres")
                 ?? "Host=localhost;Port=5432;Database=cartoncaps;Username=postgres;Password=postgres";
        services.AddDbContext<ReferralsDbContext>(opt =>
            opt.UseNpgsql(cs, npgsql =>
                npgsql.MigrationsAssembly(typeof(ReferralsDbContext).Assembly.FullName)));
        services.AddScoped<IReferralService, EfReferralService>();

        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
    }

    public void Configure(IApplicationBuilder app)
    {
        var env = _env;
        // Order: Routing -> RateLimiter -> Endpoints to honor per-endpoint policies
        var applyMigrations =
            _configuration.GetValue<bool?>("Database:ApplyMigrationsOnStartup") ?? env.IsDevelopment();
        if (applyMigrations && !env.IsEnvironment("Testing"))
        {
            using var scope = app.ApplicationServices.CreateScope();
            var ef = scope.ServiceProvider.GetRequiredService<ReferralsDbContext>();
            var before = ef.Database.GetAppliedMigrations().Count();
            var pending = ef.Database.GetPendingMigrations().ToList();
            if (pending.Count > 0)
                Log.Information("Applying {Count} EF migrations: {Migrations}", pending.Count, pending);
            ef.Database.Migrate();
            var after = ef.Database.GetAppliedMigrations().Count();
            var applied = after - before;
            Log.Information("EF Migrate complete. Applied {Applied} new migrations. Total applied: {Total}", applied,
                after);
            // Seed only when explicitly allowed (dev/local). Never in Production by default.
            var seed = _configuration.GetValue<bool?>("Database:SeedOnStartup") ?? env.IsDevelopment();
            if (seed) DbSeeder.SeedAsync(ef).GetAwaiter().GetResult();
        }

        app.UseRouting();
        app.UseRateLimiter();

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var ex = feature?.Error;
                var status = ex switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    ValidationException => StatusCodes.Status400BadRequest,
                    FV.ValidationException => StatusCodes.Status400BadRequest,
                    ForbiddenException => StatusCodes.Status403Forbidden,
                    ForbiddenOperationException => StatusCodes.Status403Forbidden,
                    ConflictException => StatusCodes.Status409Conflict,
                    ConflictOperationException => StatusCodes.Status409Conflict,
                    GoneException => StatusCodes.Status410Gone,
                    ResourceGoneException => StatusCodes.Status410Gone,
                    RateLimitedException => StatusCodes.Status429TooManyRequests,
                    BadHttpRequestException => StatusCodes.Status400BadRequest,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    _ => StatusCodes.Status500InternalServerError
                };

                var pd = new ProblemDetails
                {
                    Type = "about:blank",
                    Title = status switch
                    {
                        StatusCodes.Status400BadRequest => "Bad Request",
                        StatusCodes.Status401Unauthorized => "Unauthorized",
                        StatusCodes.Status404NotFound => "Not Found",
                        _ => "An error occurred"
                    },
                    Status = status,
                    Detail = ex?.Message,
                    Instance = context.Request.Path
                };

                Log.Error(ex, "Unhandled exception");
                context.Response.ContentType = "application/problem+json";
                context.Response.StatusCode = status;
                await context.Response.WriteAsJsonAsync(pd);
            });
        });

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Carton Caps Referrals API v1");
            options.DocumentTitle = "Carton Caps Referrals API v1";
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health");
            endpoints.MapReferralsEndpoints();
            endpoints.MapLinksEndpoints();
            endpoints.MapEventsEndpoints();
        });
    }
}
