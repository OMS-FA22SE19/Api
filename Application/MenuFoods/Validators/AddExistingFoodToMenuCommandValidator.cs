using Application.Common.Validator;
using Application.MenuFoods.Commands;
using FluentValidation;

namespace Application.MenuFoods.Validators
{
    public class AddExistingFoodToMenuCommandValidator : OMSAbstractValidator<AddExistingFoodToMenuCommand>
    {
        public AddExistingFoodToMenuCommandValidator()
        {
            RuleFor(e => e.Id)
                .NotEmpty().WithMessage("{PropertyName} is not empty");

            RuleFor(e => e.FoodId)
                .NotEmpty().WithMessage("{PropertyName} is not empty");

            RuleFor(e => e.Price)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .GreaterThan(0).WithMessage("{PropertyName} is invalid");
        }
    }
}
