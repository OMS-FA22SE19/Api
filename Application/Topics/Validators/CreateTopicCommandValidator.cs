using Application.Common.Validator;
using Application.Topics.Commands;
using FluentValidation;

namespace Application.Topics.Validators
{
    public class CreateTopicCommandValidator : OMSAbstractValidator<CreateTopicCommand>
    {
        public CreateTopicCommandValidator()
        {
            RuleFor(e => e.Name)
                .NotEmpty().WithMessage("{PropertyName} is not empty")
                .Length(2, 256).WithMessage("Length {PropertyName} must between 2 and 256")
                .Must(BeAValidName).WithMessage("{PropertyName} contains invalid characters");
        }
    }
}
