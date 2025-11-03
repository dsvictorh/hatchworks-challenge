using CartonCaps.Referrals.Api.Filters;
using CartonCaps.Referrals.Core.Application.Abstractions;
using CartonCaps.Referrals.Core.Application.Contracts;
using CartonCaps.Referrals.Core.Domain.Entities;
using CartonCaps.Referrals.Core.Domain.Enums;
using CartonCaps.Referrals.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CartonCaps.Referrals.Api.Endpoints;

/// <summary>
///     Analytics and vendor events endpoints.
/// </summary>
public static class EventsEndpoints
{
    public static IEndpointRouteBuilder MapEventsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/referrals")
            .WithTags("Events");

        group.AddEndpointFilter(new FluentValidationFilter(app.ServiceProvider));
        group.AddEndpointFilter(new ValidationFilter());

        group.MapPost("share", (IReferralService svc, ShareEventRequest req) =>
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
            .RequireRateLimiting("sharePolicy");

        group.MapPost("events", async (ReferralsDbContext db, ReferralEventIn req) =>
            {
                if (!string.IsNullOrWhiteSpace(req.EventId))
                {
                    var exists = await db.ReferralEvents.AnyAsync(e => e.EventId == req.EventId);
                    if (exists) return Results.Accepted();
                }

                var ev = new ReferralEvent
                {
                    EventType = req.Event,
                    ReferralCode = req.ReferralCode,
                    EventId = req.EventId,
                    DeviceId = req.DeviceInfo?.DeviceId,
                    Timestamp = DateTimeOffset.UtcNow
                };
                await db.ReferralEvents.AddAsync(ev);

                var code = await db.ReferralCodes.FirstOrDefaultAsync(c => c.Code == req.ReferralCode);
                if (code != null)
                {
                    var now = DateTimeOffset.UtcNow;
                    var referral = await db.Referrals.Where(r => r.ReferralCode == req.ReferralCode)
                        .OrderByDescending(r => r.InvitedAt).FirstOrDefaultAsync();
                    if (referral == null)
                    {
                        referral = new Referral
                        {
                            ReferrerId = code.UserId,
                            ReferralCode = req.ReferralCode,
                            InvitedAt = now
                        };
                        await db.Referrals.AddAsync(referral);
                    }

                    referral.Status = ReferralStateMachine.Advance(referral.Status, req.Event);
                }

                await db.SaveChangesAsync();
                return Results.Accepted();
            })
            .WithName("ProcessReferralEvent")
            .WithSummary("Process events");

        return app;
    }
}
