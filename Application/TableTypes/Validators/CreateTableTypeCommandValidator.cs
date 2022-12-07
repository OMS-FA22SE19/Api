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
                .Length(2, 256).WithMessage("Length {PropertyName} must be between 2 and 256")
                .Must(BeAValidName).WithMessage("{PropertyName} contains invalid characters");

            RuleFor(e => e.ChargePerSeat)
                .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be greater than 0");

            RuleFor(e => e.CanBeCombined)
                .NotNull().WithMessage("{PropertyName} is not empty");
        }
    }
}