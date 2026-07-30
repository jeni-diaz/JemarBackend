using FluentValidation;
using System.Text.RegularExpressions;

namespace Jemar.Aplication.Validation
{
    public static class NameRules
    {
        public static IRuleBuilderOptions<T, string> MustBeValidName<T>(this IRuleBuilderInitial<T, string> ruleBuilder, string fieldLabel) =>
            ruleBuilder
                .Cascade(CascadeMode.Stop)
                .Must(value => !string.IsNullOrWhiteSpace(value) && value.Trim().Length > 3)
                    .WithMessage($"El {fieldLabel} debe tener más de 3 letras.")
                .Must(value => Regex.IsMatch(value.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$"))
                    .WithMessage($"El {fieldLabel} solo puede contener letras.");
    }
}
