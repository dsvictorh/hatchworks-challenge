using CartonCaps.Referrals.Core.Application.Contracts;
using FluentValidation;

namespace CartonCaps.Referrals.Api.Validators;

public class ShareMessageRequestValidator : AbstractValidator<ShareMessageRequest>
{
    public ShareMessageRequestValidator()
    {
        RuleFor(x => x.Channel)
            .NotEmpty()
            .Must(c => new[] { "sms", "email", "generic" }.Contains(c))
            .WithMessage("Channel must be one of: sms, email, generic");

        RuleFor(x => x.Locale)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.Locale));
    }
}
