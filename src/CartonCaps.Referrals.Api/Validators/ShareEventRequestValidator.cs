using CartonCaps.Referrals.Core.Application.Contracts;
using FluentValidation;

namespace CartonCaps.Referrals.Api.Validators;

public class ShareEventRequestValidator : AbstractValidator<ShareEventRequest>
{
    public ShareEventRequestValidator()
    {
        RuleFor(x => x.Channel).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Link).NotEmpty().MaximumLength(2048);

        When(x => x.DeviceInfo != null,
            () => { RuleFor(x => x.DeviceInfo!.DeviceId).NotEmpty().When(x => x.DeviceInfo != null); });
    }
}
