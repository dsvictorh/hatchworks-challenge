using CartonCaps.Referrals.Api.Filters;
using CartonCaps.Referrals.Core.Application.Abstractions;
using CartonCaps.Referrals.Core.Application.Contracts;
using Serilog;

namespace CartonCaps.Referrals.Api.Endpoints;

/// <summary>
///     Link generation and share-message endpoints.
/// </summary>
public static class LinksEndpoints
{
    public static IEndpointRouteBuilder MapLinksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/v1/referrals")
            .WithTags("Links");

        group.AddEndpointFilter(new FluentValidationFilter(app.ServiceProvider));
        group.AddEndpointFilter(new ValidationFilter());

        group.MapPost("link", (IReferralService svc, GenerateLinkRequest req) =>
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
            .RequireRateLimiting("linkPolicy");

        group.MapPost("share-message", (IReferralService svc, ShareMessageRequest req) =>
            {
                var userId = "usr_referrer_001";

                try
                {
                    var res = svc.GetShareMessage(userId, req.Channel, req.Locale);
                    return Results.Ok(res);
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Message template failed");
                    throw;
                }
            })
            .WithName("GetShareMessage")
            .WithSummary("Get share message");

        return app;
    }
}
