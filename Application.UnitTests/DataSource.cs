using Core.Entities;
using Core.Enums;

namespace Application.UnitTests
{
    public class DataSource
    {
        public static List<CourseType> CourseTypes
            => new List<CourseType>()
        {
                new CourseType()
                {
                    Id = 1,
                    Name = "Others"
                },
                new CourseType()
                {
                    Id = 2,
                    Name = "Starters"
                },
                new CourseType()
                {
                    Id = 3,
                    Name = "Main Courses"
                },
                new CourseType()
                {
                    Id = 4,
                    Name = "Desserts"
                },
                new CourseType()
                {
                    Id = 5,
                    Name = "Drinks"
                }
        };

        public static List<TableType> TableTypes
            => new List<TableType>()
        {
                new TableType()
                {
                    Id = 1,
                    Name = "Indoor",
                    ChargePerSeat = 10000,
                    CanBeCombined = true
                },
                new TableType()
                {
                    Id = 2,
                    Name = "Outdoor",
                    ChargePerSeat = 20000,
                    CanBeCombined = true
                },
                new TableType()
                {
                    Id = 3,
                    Name = "Room",
                    ChargePerSeat = 15000,
                    CanBeCombined = false
                }
        };

        public static List<Table> Tables
            => new List<Table>()
        {
                new Table()
                {
                    Id = 1,
                    NumOfSeats = 4,
                    Status = TableStatus.Available,
                    TableTypeId = 1
                },
                new Table()
                {
                    Id = 2,
                    NumOfSeats = 6,
                    Status = TableStatus.Available,
                    TableTypeId = 2
                },
                new Table()
                {
                    Id = 3,
                    NumOfSeats = 8,
                    Status = TableStatus.Available,
                    TableTypeId = 3
                },
                new Table()
                {
                    Id = 4,
                    NumOfSeats = 4,
                    Status = TableStatus.Available,
                    TableTypeId = 1
                },
                new Table()
                {
                    Id = 5,
                    NumOfSeats = 8,
                    Status = TableStatus.Available,
                    TableTypeId = 1
                }
        };
    }
}
