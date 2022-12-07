﻿using Application.Common.Validator;
using Application.Reservations.Commands;
using FluentValidation;

namespace Application.Reservations.Validators
{
    public class CreateReservationCommandValidator : OMSAbstractValidator<CreateReservationCommand>
    {
        public CreateReservationCommandValidator()
        {
            RuleFor(e => e.StartTime)
                .NotNull().WithMessage("{PropertyName} is not null")
                .Must(BeAFutureDate).WithMessage("{PropertyName} is invalid");

            RuleFor(e => e.EndTime)
                .NotNull().WithMessage("{PropertyName} is not null")
                .GreaterThan(e => e.StartTime).WithMessage("{PropertyName} must be after StartTime");

            RuleFor(e => e.NumOfPeople)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .GreaterThan(0).WithMessage("{PropertyName} should be greater than 0");

            RuleFor(e => e.NumOfSeats)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .GreaterThan(0).WithMessage("{PropertyName} should be greater than 0");

            RuleFor(e => e.TableTypeId)
                .NotEmpty().WithMessage("{PropertyName} is not empty");

            RuleFor(e => e.Quantity)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .GreaterThan(0).WithMessage("{PropertyName} should be greater than 0");
        }
    }
}