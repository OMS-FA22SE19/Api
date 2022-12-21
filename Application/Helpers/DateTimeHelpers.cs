using Application.Common.Exceptions;
using Application.Reservations.Response;
using Core.Entities;
using System.Collections.Immutable;

namespace Application.Helpers
{
    public static class DateTimeHelpers
    {
        public static (bool isValid, string errorMessage) ValidateDateInAvailableTime(DateTime startTime, DateTime endTime, List<AdminSetting> settings)
        {
            var availableStartTime = settings.FirstOrDefault(e => e.Name.Equals("StartTime"));
            if (availableStartTime is null || !DateTime.TryParse(availableStartTime?.Value, out DateTime startTimeLimit))
            {
                return (false, "Invalid format");
            }
            if (startTime.TimeOfDay < startTimeLimit.TimeOfDay)
            {
                return (false, $"StartTime must be after {startTimeLimit.TimeOfDay}");
            }

            var availableEndTime = settings.FirstOrDefault(e => e.Name.Equals("EndTime"));
            if (availableEndTime is null || !DateTime.TryParse(availableEndTime?.Value, out DateTime endTimeLimit))
            {
                return (false, "Invalid format");
            }
            if (endTime.TimeOfDay > endTimeLimit.TimeOfDay)
            {
                return (false, $"EndTime must be after {endTimeLimit.TimeOfDay}");
            }

            return (true, string.Empty);
        }

        public static (bool isValid, string errorMessage) ValidateStartEndTime(DateTime startTime, DateTime endTime, int quantity, List<Reservation> reservations, int maxTables, List<AdminSetting> settings, bool isDefaultCustomer = true)
        {
            var busyTimes = GetBusyDateOfTable(quantity, reservations, maxTables, settings);

            return busyTimes.All(e => startTime >= e.EndTime || endTime <= e.StartTime) ? (true, string.Empty) : (false, "This reservation is unavailable! Please try again!");
        }

        public static List<BusyTimeDto> GetBusyDateOfTable(int quantity, List<Reservation> reservations, int maxTables, List<AdminSetting> settings, bool isDefaultCustomer = true)
        {
            var busyTimes = new List<BusyTimeDto>();
            double availablePercentage = 1;
            var reservationTable = settings.FirstOrDefault(e => e.Name.Equals("ReservationTable"));
            if (reservationTable is not null && double.TryParse(reservationTable?.Value, out double reservationTablePercentage) && !isDefaultCustomer)
            {
                availablePercentage = reservationTablePercentage / 100;
            }
            var availalbleTables = maxTables * availablePercentage - quantity;

            if (availalbleTables < 0)
            {
                throw new BadRequestException("Insufficent amount of tables");
            }

            var times = reservations.Select(e => e.StartTime).Concat(reservations.Select(e => e.EndTime)).ToImmutableSortedSet();

            for (int i = 0; i < (times.Count - 1); i++)
            {
                var count = reservations.Where(e => e.StartTime <= times[i] && e.EndTime >= times[i + 1]).AsEnumerable().Sum(e => e.Quantity);
                if (availalbleTables - count >= 0)
                {
                    continue;
                }
                var time = busyTimes.FirstOrDefault(e => e.EndTime == times[i]);
                if (time is null)
                {
                    busyTimes.Add(new BusyTimeDto
                    {
                        StartTime = times[i],
                        EndTime = times[i + 1]
                    });
                }
                else
                {
                    time.EndTime = times[i + 1];
                }
            }

            return busyTimes;
        }
    }
}
