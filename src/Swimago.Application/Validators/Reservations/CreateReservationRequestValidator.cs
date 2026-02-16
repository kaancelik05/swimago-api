using FluentValidation;
using Swimago.Application.DTOs.Reservations;

namespace Swimago.Application.Validators.Reservations;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.ListingId)
            .NotEmpty().WithMessage("Geçerli bir liste seçiniz.");

        RuleFor(x => x.BookingType)
            .IsInEnum().WithMessage("Geçerli bir rezervasyon türü seçiniz.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Başlangıç tarihi gereklidir.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddHours(-1)).WithMessage("Geçmiş tarih için rezervasyon yapılamaz.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("Bitiş tarihi gereklidir.")
            .GreaterThan(x => x.StartTime).WithMessage("Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");

        RuleFor(x => x)
            .Must(x => (x.EndTime - x.StartTime).TotalHours >= 1)
            .WithMessage("Rezervasyon süresi en az 1 saat olmalıdır.");

        RuleFor(x => x.GuestCount)
            .GreaterThan(0).WithMessage("Misafir sayısı en az 1 olmalıdır.")
            .LessThanOrEqualTo(50).WithMessage("Misafir sayısı en fazla 50 olabilir.");

        RuleFor(x => x.SpecialRequests)
            .MaximumLength(1000).WithMessage("Özel istekler en fazla 1000 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.SpecialRequests));
    }
}
