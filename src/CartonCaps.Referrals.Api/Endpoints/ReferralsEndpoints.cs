using CartonCaps.Referrals.Api.Filters;
using CartonCaps.Referrals.Core.Application.Abstractions;
using CartonCaps.Referrals.Core.Application.Contracts;
using Serilog;

namespace CartonCaps.Referrals.Api.Endpoints;

/// <summary>
///     Referral-related API endpoints.
/// </summary>
public static class ReferralsEndpoints
{
    public static IEndpointRouteBuilder MapReferralsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/referrals")
            .WithTags("Referrals");

        // Apply request validation to all endpoints in this group
        group.AddEndpointFilter(new FluentValidationFilter(app.ServiceProvider));
        group.AddEndpointFilter(new ValidationFilter());

        group.MapGet(string.Empty, (HttpContext ctx, IReferralService svc, int? page, int? size, string? status) =>
            {
                var userId = "usr_referrer_001"; // simulates authenticated user

                try
                {
                    var p = page.HasValue && page.Value > 0 ? page.Value : 1;
                    var sz = size.HasValue && size.Value > 0 ? size.Value : 20;
                    var result = svc.GetMyReferrals(userId, p, sz, string.IsNullOrWhiteSpace(status) ? null : status);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    Log.Warning("Error getting referrals: {Error}", ex.Message);
                    throw;
                }
            })
            .WithName("GetReferrals")
            .WithSummary("Get referrals");

        // Links and Events moved to dedicated modules

        group.MapPost("verify", (IReferralService svc, VerifyReferralRequest req) =>
            {
                var res = svc.Verify(req);
                return Results.Ok(res);
            })
            .WithName("VerifyReferralCode")
            .WithSummary("Verify code");

        group.MapPost("session", (IReferralService svc, CreateSessionRequest req) =>
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
            .WithTags("Referrals");

        group.MapPatch("redeem", (IReferralService svc, RedeemReferralRequest req) =>
            {
                var userId = "usr_backend_system";

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
            .WithSummary("Redeem referral");

        return app;
    }
}
