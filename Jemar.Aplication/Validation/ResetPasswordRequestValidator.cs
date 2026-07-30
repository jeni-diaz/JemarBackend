using FluentValidation;
using Jemar.Aplication.Requests;
using System.Text.RegularExpressions;

namespace Jemar.Aplication.Validation
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .Must(email => !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    .WithMessage("El email no tiene un formato válido.");

            RuleFor(x => x.Code).NotEmpty().WithMessage("El código es requerido.");

            RuleFor(x => x.NewPassword).MustBeStrongPassword();
        }
    }
}
