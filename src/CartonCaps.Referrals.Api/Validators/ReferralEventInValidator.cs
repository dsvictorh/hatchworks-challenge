using CartonCaps.Referrals.Core.Application.Contracts;
using FluentValidation;

namespace CartonCaps.Referrals.Api.Validators;

public class ReferralEventInValidator : AbstractValidator<ReferralEventIn>
{
    public ReferralEventInValidator()
    {
        RuleFor(x => x.Event)
            .NotEmpty()
            .Must(e => new[] { "click", "install", "open", "registered", "redeemed" }.Contains(e))
            .WithMessage("Event must be one of: click, install, open, registered, redeemed");

        RuleFor(x => x.ReferralCode)
            .NotEmpty()
            .Length(6, 20)
            .Matches("^[A-Za-z0-9]+$");

        RuleFor(x => x.EventId)
            .MaximumLength(64)
            .When(x => !string.IsNullOrWhiteSpace(x.EventId));
    }
}
