using Application.Common.Validator;
using Application.Types.Commands;
using FluentValidation;

namespace Application.Types.Validators
{
    public class UpdateTypeCommandValidator : OMSAbstractValidator<UpdateTypeCommand>
    {
        public UpdateTypeCommandValidator()
        {
            RuleFor(e => e.Name)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 1000).WithMessage("Length {PropertyName} must between 2 and 1000")
                .Must(BeAValidName).WithMessage("{PropertyName} contains invalid characters");

            RuleFor(e => e.Description)
                .NotEmpty().WithMessage("{PropertyName} is not empty");
        }
    }
}
