using Application.Common.Validator;
using Application.Tables.Commands;
using FluentValidation;

namespace Application.Reservations.Validators
{
    public class CreateTableCommandValidator : OMSAbstractValidator<CreateTableCommand>
    {
        public CreateTableCommandValidator()
        {
            RuleFor(e => e.NumOfSeats)
                .NotNull().WithMessage("{PropertyName} is not null")
                .GreaterThan(0).WithMessage("{PropertyName} should be greater than 0");

            RuleFor(e => e.TableTypeId)
                .NotNull().WithMessage("{PropertyName} is not null")
                .GreaterThan(0).WithMessage("{PropertyName} should be greater than 0");
        }
    }
}