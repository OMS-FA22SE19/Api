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
                    Name = "Starters"
                },
                new CourseType()
                {
                    Id = 2,
                    Name = "Main Courses"
                },
                new CourseType()
                {
                    Id = 3,
                    Name = "Desserts"
                },
                new CourseType()
                {
                    Id = 4,
                    Name = "Drinks"
                },
                new CourseType()
                {
                    Id = 5,
                    Name = "Others"
                },
        };
    }
}
