using System.Text.RegularExpressions;
using DTOs.Requests;
using FluentValidation;

namespace Magik_Johnson.DTOs.Requests.Validations;

public class AddressRegistrationRequestValidator : AbstractValidator<AddressRegistrationRequest>
{
    public AddressRegistrationRequestValidator()
    {
        RuleFor(r => r.DoorNb).NotNull().NotEmpty().GreaterThan(0);
        RuleFor(r => r.Street)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .Matches(@"^[\p{L} ,.'-]+$", RegexOptions.IgnoreCase);
        RuleFor(r => r.City)
            .NotNull()
            .NotEmpty()
            .Length(1, 100)
            .Matches(@"^[\p{L} ,.'-]+$", RegexOptions.IgnoreCase);
        RuleFor(r => r.PostalCode).NotNull().NotEmpty().Matches(@"^[A-Z]\d[A-Z] \d[A-Z]\d$");
        RuleFor(r => r.ProvinceId).NotNull().NotEmpty();
        RuleFor(r => r.CountryId).NotNull().NotEmpty();
    }
}
