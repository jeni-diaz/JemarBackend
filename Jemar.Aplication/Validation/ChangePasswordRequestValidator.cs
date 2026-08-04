using FluentValidation;
using Jemar.Aplication.Requests;

namespace Jemar.Aplication.Validation
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("La contraseña actual es requerida.");
            RuleFor(x => x.NewPassword).MustBeStrongPassword();
        }
    }
}
