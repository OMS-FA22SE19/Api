﻿using Application.Common.Validator;
using Application.Menus.Commands;
using FluentValidation;

namespace Application.Foods.Validators
{
    public class AddNewFoodToMenuCommandValidator : OMSAbstractValidator<AddNewFoodToMenuCommand>
    {
        public AddNewFoodToMenuCommandValidator()
        {
            RuleFor(e => e.Id)
                .NotEmpty().WithMessage("{PropertyName} is not empty");

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

            RuleFor(e => e.Picture)
                .NotEmpty().WithMessage("{PropertyName} is not empty");

            RuleFor(e => e.CourseTypeId)
                .NotEmpty().WithMessage("{PropertyName} is not empty");
        }
    }
}