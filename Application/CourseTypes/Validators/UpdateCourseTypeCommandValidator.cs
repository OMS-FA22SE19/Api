using Application.Common.Validator;
using Application.CourseTypes.Commands;
using FluentValidation;

namespace Application.CourseTypes.Validators
{
    public class UpdateCourseTypeCommandValidator : OMSAbstractValidator<UpdateCourseTypeCommand>
    {
        public UpdateCourseTypeCommandValidator()
        {
            RuleFor(e => e.Name)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 256).WithMessage("Length {PropertyName} must between 2 and 256")
                .Must(BeAValidName).WithMessage("{PropertyName} contains invalid characters");

            RuleFor(e => e.Description)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 256).WithMessage("Length {PropertyName} must between 2 and 256");
        }
    }
}