using FluentValidation;
using Swimago.Application.DTOs.Reviews;

namespace Swimago.Application.Validators.Reviews;

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.ListingId)
            .NotEmpty().WithMessage("Geçerli bir liste seçiniz.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Puan 1 ile 5 arasında olmalıdır.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Yorum gereklidir.")
            .MinimumLength(10).WithMessage("Yorum en az 10 karakter olmalıdır.")
            .MaximumLength(2000).WithMessage("Yorum en fazla 2000 karakter olabilir.");
    }
}
