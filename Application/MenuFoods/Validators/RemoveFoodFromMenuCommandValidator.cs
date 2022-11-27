using Application.Common.Validator;
using Application.MenuFoods.Commands;
using FluentValidation;

namespace Application.MenuFoods.Validators
{
    public class RemoveFoodFromMenuCommandValidator : OMSAbstractValidator<RemoveFoodFromMenuCommand>
    {
        public RemoveFoodFromMenuCommandValidator()
        {
            RuleFor(e => e.MenuId)
                .NotEmpty().WithMessage("{PropertyName} is not empty");

            RuleFor(e => e.FoodId)
                .NotEmpty().WithMessage("{PropertyName} is not empty");
        }
    }
}
