using Application.Common.Validator;
using Application.Menus.Commands;
using FluentValidation;

namespace Application.Foods.Validators
{
    public class UpdateMenuCommandValidator : OMSAbstractValidator<UpdateMenuCommand>
    {
        public UpdateMenuCommandValidator()
        {
            RuleFor(e => e.Id)
                .NotEmpty().WithMessage("{PropertyName} is not empty");

            RuleFor(e => e.Name)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 100).WithMessage("Length {PropertyName} must between 2 and 256")
                .Must(BeAValidName).WithMessage("{PropertyName} contains invalid characters");

            RuleFor(e => e.Description)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 1000).WithMessage("Length {PropertyName} must between 2 and 256");
        }
    }
}