using Application.Common.Validator;
using Application.Foods.Commands;
using FluentValidation;

namespace Application.Foods.Validators
{
    public class UpdateFoodCommandValidator : OMSAbstractValidator<UpdateFoodCommand>
    {
        public UpdateFoodCommandValidator()
        {
            RuleFor(e => e.Name)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 1000).WithMessage("Length {PropertyName} must between 2 and 256")
                .Must(BeAValidName).WithMessage("{PropertyName} contains invalid characters");

            RuleFor(e => e.Description)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 4000).WithMessage("Length {PropertyName} must between 2 and 256");

            RuleFor(e => e.Ingredient)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 2000).WithMessage("Length {PropertyName} must between 2 and 256");

            RuleFor(e => e.CourseTypeId)
                .NotEmpty().WithMessage("{PropertyName} is not empty");
        }
    }
}