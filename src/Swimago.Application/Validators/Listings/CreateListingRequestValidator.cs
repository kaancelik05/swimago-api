using FluentValidation;
using Swimago.Application.DTOs.Listings;

namespace Swimago.Application.Validators.Listings;

public class CreateListingRequestValidator : AbstractValidator<CreateListingRequest>
{
    public CreateListingRequestValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Title).NotEmpty().Must(x => x.ContainsKey("tr")).WithMessage("Title must contain at least Turkish version");
        RuleFor(x => x.Description).NotEmpty().Must(x => x.ContainsKey("tr")).WithMessage("Description must contain at least Turkish version");
        RuleFor(x => x.Address).NotEmpty().Must(x => x.ContainsKey("tr")).WithMessage("Address must contain at least Turkish version");
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.MaxGuestCount).GreaterThan(0);
        RuleFor(x => x.ImageUrls).NotEmpty();
    }
}
