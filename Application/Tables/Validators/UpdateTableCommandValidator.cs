using Application.Common.Validator;
using Application.Tables.Commands;
using FluentValidation;

namespace Application.Tables.Validators
{
    public class UpdateTableCommandValidator : OMSAbstractValidator<UpdateTableCommand>
    {
        public UpdateTableCommandValidator()
        {
            RuleFor(e=>e.Id)
                .NotEmpty().WithMessage("{PropertyName} is not null");

            RuleFor(e => e.NumOfSeats)
                .NotNull().WithMessage("{PropertyName} is not null")
                .GreaterThan(0).WithMessage("{PropertyName} should be greater than 0");

            RuleFor(e => e.TableTypeId)
                .NotNull().WithMessage("{PropertyName} is not null")
                .GreaterThan(0).WithMessage("{PropertyName} should be greater than 0");
        }
    }
}