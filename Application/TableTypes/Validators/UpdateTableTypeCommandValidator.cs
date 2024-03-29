﻿using Application.Common.Validator;
using Application.TableTypes.Commands;
using FluentValidation;

namespace Application.TableTypes.Validators
{
    public class UpdateTableTypeCommandValidator : OMSAbstractValidator<UpdateTableTypeCommand>
    {
        public UpdateTableTypeCommandValidator()
        {
            RuleFor(e => e.Id)
                .NotEmpty().WithMessage("{PropertyName} is not empty");

            RuleFor(e => e.Name)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 256).WithMessage("Length {PropertyName} must between 2 and 256")
                .Must(BeAValidName).WithMessage("{PropertyName} contains invalid characters");

            RuleFor(e => e.ChargePerSeat)
                .NotNull().WithMessage("{PropertyName} is not empty")
                .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} is invalid");

            RuleFor(e => e.CanBeCombined)
                .NotNull().WithMessage("{PropertyName} is not null");
        }
    }
}