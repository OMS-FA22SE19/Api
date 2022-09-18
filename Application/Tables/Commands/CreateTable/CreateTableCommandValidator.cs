using FluentValidation;

namespace Application.Tables.Commands.CreateTable
{
    public class CreateTableCommandValidator : AbstractValidator<CreateTableCommand>
    {
        public CreateTableCommandValidator()
        {
            RuleFor(e => e.NumOfSeats)
                .GreaterThan(0)
                .LessThan(1000)
                .WithMessage("Number of seats must be greater than 0 and less than 1000");

            RuleFor(e => e.Status)
                .IsInEnum();
        }
    }
}
