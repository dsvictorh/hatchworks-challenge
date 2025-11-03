using CartonCaps.Referrals.Core.Application.Contracts;
using FluentValidation;

namespace CartonCaps.Referrals.Api.Validators;

public class GenerateLinkRequestValidator : AbstractValidator<GenerateLinkRequest>
{
    public GenerateLinkRequestValidator()
    {
        RuleFor(x => x.Channel)
            .NotEmpty()
            .Must(c => c == "sms" || c == "email" || c == "generic")
            .WithMessage("Channel must be one of: sms, email, generic");
    }
}
