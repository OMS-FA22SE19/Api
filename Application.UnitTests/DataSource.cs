using Core.Entities;

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
    }
}
