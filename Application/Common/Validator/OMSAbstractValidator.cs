using FluentValidation;
using System;
using System.Linq;

namespace Application.Common.Validator
{
    public class OMSAbstractValidator<T> : AbstractValidator<T>
    {
        protected virtual bool BeAValidName(string name)
        {
            name = name.Replace("  ", "");
            name = name.Replace("-", "");
            return name.All(Char.IsLetter);
        }

        protected virtual bool BeAFutureDate(DateTime date) => date == default(DateTime) ? false : (date <= DateTime.Today);

        protected virtual bool BeAPastDate(DateTime date) => date == default(DateTime) ? false : (date <= DateTime.Today);

        protected virtual bool BeAPresentDate(DateTime date) => date != default(DateTime) && (date >= DateTime.Today.AddDays(-1) && date <= DateTime.Today.AddDays(1));
        protected virtual bool BeAPresentYear(DateTime date) => date.Year == DateTime.Now.Year;
    }
}