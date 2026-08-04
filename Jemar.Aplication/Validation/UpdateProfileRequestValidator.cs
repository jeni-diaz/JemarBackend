using FluentValidation;
using Jemar.Aplication.Requests;
using System.Text.RegularExpressions;

namespace Jemar.Aplication.Validation
{
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.FirstName).MustBeValidName("nombre");
            RuleFor(x => x.LastName).MustBeValidName("apellido");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .Must(email => !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    .WithMessage("El email no tiene un formato válido.");
        }
    }
}
