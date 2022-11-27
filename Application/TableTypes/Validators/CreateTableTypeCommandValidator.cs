using Application.Common.Validator;
using Application.TableTypes.Commands;
using FluentValidation;

namespace Application.TableTypes.Validators
{
    public class CreateTableTypeCommandValidator : OMSAbstractValidator<CreateTableTypeCommand>
    {
        public CreateTableTypeCommandValidator()
        {
            RuleFor(e => e.Name)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 256).WithMessage("Length {PropertyName} must between 2 and 256")
                .Must(BeAValidName).WithMessage("{PropertyName} contains invalid characters");

            RuleFor(e => e.ChargePerSeat)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .GreaterThan(0).WithMessage("{PropertyName} is invalid");

            RuleFor(e => e.CanBeCombined)
                .NotEmpty().WithMessage("{PropertyName} is not empty");
        }
    }
}