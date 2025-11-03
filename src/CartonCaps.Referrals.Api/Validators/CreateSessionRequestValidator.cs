using CartonCaps.Referrals.Core.Application.Contracts;
using FluentValidation;

namespace CartonCaps.Referrals.Api.Validators;

public class CreateSessionRequestValidator : AbstractValidator<CreateSessionRequest>
{
    public CreateSessionRequestValidator()
    {
        RuleFor(x => x.ReferralCode)
            .NotEmpty()
            .Length(6, 20)
            .Matches("^[A-Za-z0-9]+$");

        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .Length(3, 100);
    }
}
