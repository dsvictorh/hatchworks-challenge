using CartonCaps.Referrals.Core.Application.Contracts;
using FluentValidation;

namespace CartonCaps.Referrals.Api.Validators;

public class RedeemReferralRequestValidator : AbstractValidator<RedeemReferralRequest>
{
    public RedeemReferralRequestValidator()
    {
        RuleFor(x => x.ReferralCode)
            .NotEmpty()
            .Length(6, 20)
            .Matches("^[A-Za-z0-9]+$");

        RuleFor(x => x.RefereeUserId)
            .NotEmpty()
            .Length(5, 50);
    }
}
