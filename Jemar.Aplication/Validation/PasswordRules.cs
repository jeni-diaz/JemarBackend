using FluentValidation;
using System.Text.RegularExpressions;

namespace Jemar.Aplication.Validation
{
    public static class PasswordRules
    {
        public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(this IRuleBuilderInitial<T, string> ruleBuilder) =>
            ruleBuilder
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("La contraseña es requerida.")
                .Must(HaveEnoughLettersAndDigits).WithMessage("La contraseña debe tener al menos 3 letras y 1 número.");

        private static bool HaveEnoughLettersAndDigits(string password)
        {
            var letters = Regex.Matches(password, "[a-zA-Z]").Count;
            var digits = Regex.Matches(password, "[0-9]").Count;
            return letters >= 3 && digits >= 1;
        }
    }
}
