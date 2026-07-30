using FluentValidation;
using Jemar.Aplication.Requests;
using Jemar.Domain.Enums;
using System.Text.RegularExpressions;

namespace Jemar.Aplication.Validation
{
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.FirstName).MustBeValidName("nombre");
            RuleFor(x => x.LastName).MustBeValidName("apellido");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .Must(email => !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    .WithMessage("El email no tiene un formato válido.");

            RuleFor(x => x.Password).MustBeStrongPassword();

            RuleFor(x => x.Role)
                .Must(role => Enum.IsDefined(typeof(UserRoleEnum), role))
                .WithMessage("El rol especificado no es válido.");
        }
    }
}
