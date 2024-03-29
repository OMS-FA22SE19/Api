﻿using Application.Common.Validator;
using Application.Menus.Commands;
using FluentValidation;

namespace Application.Menus.Validators
{
    public class AddNewFoodToMenuCommandValidator : OMSAbstractValidator<AddNewFoodToMenuCommand>
    {
        public AddNewFoodToMenuCommandValidator()
        {
            RuleFor(e => e.Id)
                .NotEmpty().WithMessage("{PropertyName} is not empty");

            RuleFor(e => e.Name)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 1000).WithMessage("Length {PropertyName} must between 2 and 1000")
                .Must(BeAValidName).WithMessage("{PropertyName} contains invalid characters");

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