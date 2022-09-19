using Application.Common.Interfaces;

namespace Infrastructure.Services
{
    public sealed class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.UtcNow;
    }
}
