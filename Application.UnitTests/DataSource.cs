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

        public static List<Core.Entities.Type> Types
            => new List<Core.Entities.Type>()
        {
                new Core.Entities.Type()
                {
                    Id = 1,
                    Name = "Diary"
                },
                new Core.Entities.Type()
                {
                    Id = 2,
                    Name = "Fruit and vegetables"
                },
                new Core.Entities.Type()
                {
                    Id = 3,
                    Name = "Protein"
                },
                new Core.Entities.Type()
                {
                    Id = 4,
                    Name = "Fat"
                },
                new Core.Entities.Type()
                {
                    Id = 5,
                    Name = "Starchy food"
                }
        };

        public static List<Menu> Menus
            => new List<Menu>()
        {
                new Menu()
                {
                    Id = 1,
                    Name = "Main Menu",
                    Description = "This is the main menu of the restaurant",
                    Available = false,
                    IsDeleted = false
                },
                new Menu()
                {
                    Id = 2,
                    Name = "Hidden Menu",
                    Description = "This is the hidden menu of the restaurant",
                    Available = true,
                    IsDeleted = false
                },
                new Menu()
                {
                    Id = 1,
                    Name = "Deleted Menu",
                    Description = "This is the deleted menu of the restaurant",
                    Available = false,
                    IsDeleted = true
                }
        };

        public static List<Food> Foods
            => new List<Food>()
        {
                new Food()
                {
                    Id = 1,
                    Name = "Mi",
                    Description = "Mi",
                    Ingredient = "Trung",
                    Available = true,
                    PictureUrl = "picture",
                    CourseTypeId = 3
                },
                new Food()
                {
                    Id = 2,
                    Name = "Sup",
                    Description = "Sup",
                    Ingredient = "Trung",
                    Available = true,
                    PictureUrl = "picture",
                    CourseTypeId = 2
                },
                new Food()
                {
                    Id = 3,
                    Name = "Kem",
                    Description = "Kem",
                    Ingredient = "Sua",
                    Available = true,
                    PictureUrl = "picture",
                    CourseTypeId = 4
                },
                new Food()
                {
                    Id = 4,
                    Name = "Cam ep",
                    Description = "Cam ep",
                    Ingredient = "Cam",
                    Available = true,
                    PictureUrl = "picture",
                    CourseTypeId = 5
                }
        };
        public static List<MenuFood> MenuFoods
            => new List<MenuFood>()
        {
                new MenuFood()
                {
                    MenuId = 1,
                    FoodId = 1,
                    Price = 100000
                },
                new MenuFood()
                {
                    MenuId = 1,
                    FoodId = 2,
                    Price = 50000
                },
                new MenuFood()
                {
                    MenuId = 1,
                    FoodId = 3,
                    Price = 30000
                },
                new MenuFood()
                {
                    MenuId = 1,
                    FoodId = 4,
                    Price = 10000
                }
        };

        public static List<FoodType> FoodTypes
            => new List<FoodType>()
        {
                new FoodType()
                {
                    FoodId = 1,
                    TypeId = 4
                },
                new FoodType()
                {
                    FoodId = 2,
                    TypeId = 3
                },
                new FoodType()
                {
                    FoodId = 3,
                    TypeId = 4
                },
                new FoodType()
                {
                    FoodId = 4,
                    TypeId = 2
                }
        };
    }
}
