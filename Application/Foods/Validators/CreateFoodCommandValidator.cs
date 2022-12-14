using Application.Common.Validator;
using Application.Foods.Commands;
using FluentValidation;

namespace Application.Foods.Validators
{
    public class CreateFoodCommandValidator : OMSAbstractValidator<CreateFoodCommand>
    {
        public CreateFoodCommandValidator()
        {
            RuleFor(e => e.Name)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 1000).WithMessage("Length {PropertyName} must between 2 and 1000");

            RuleFor(e => e.Description)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 4000).WithMessage("Length {PropertyName} must between 2 and 4000");

            RuleFor(e => e.Ingredient)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 2000).WithMessage("Length {PropertyName} must between 2 and 2000");

            RuleFor(e => e.Picture)
                .NotEmpty().WithMessage("{PropertyName} is not empty");

            RuleFor(e => e.CourseTypeId)
                .NotEmpty().WithMessage("{PropertyName} is not empty");
        }
    }
}