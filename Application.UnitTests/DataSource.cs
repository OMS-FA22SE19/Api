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
                    Available = true,
                    IsDeleted = false
                },
                new Menu()
                {
                    Id = 2,
                    Name = "Hidden Menu",
                    Description = "This is the hidden menu of the restaurant",
                    Available = false,
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
                    IsDeleted = false,
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
                    IsDeleted = false,
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
                    IsDeleted = false,
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
                    IsDeleted = false,
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

        public static List<Reservation> Reservations
            => new List<Reservation>()
        {
                new Reservation()
                {
                    Id = 1,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    NumOfPeople = 4,
                    NumOfSeats = 4,
                    TableTypeId = 1,
                    Quantity = 1,
                    Status = ReservationStatus.Reserved,
                    UserId = "123",
                    NumOfEdits = 0
                },
                new Reservation()
                {
                    Id = 2,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    NumOfPeople = 4,
                    NumOfSeats = 4,
                    TableTypeId = 1,
                    Quantity = 1,
                    Status = ReservationStatus.Available,
                    UserId = "123",
                    NumOfEdits = 3
                },
                new Reservation()
                {
                    Id = 3,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    NumOfPeople = 4,
                    NumOfSeats = 4,
                    TableTypeId = 1,
                    Quantity = 1,
                    Status = ReservationStatus.CheckIn,
                    UserId = "123",
                    NumOfEdits = 1
                },
                new Reservation()
                {
                    Id = 4,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    NumOfPeople = 4,
                    NumOfSeats = 8,
                    TableTypeId = 1,
                    Quantity = 1,
                    Status = ReservationStatus.CheckIn,
                    UserId = "123",
                    NumOfEdits = 3
                },
        };

        public static List<ReservationTable> ReservationTables
            => new List<ReservationTable>()
            {
                new ReservationTable()
                {
                    ReservationId = 3,
                    TableId = 1
                },

                new ReservationTable()
                {
                    ReservationId = 4,
                    TableId = 2
                }
            };

        public static List<Billing> Billings
            => new List<Billing>()
            {
                new Billing()
                {
                    Id = "1",
                    ReservationId = 3,
                    ReservationAmount = 40000
                }
            };

        public static List<Order> Orders
            => new List<Order>()
            {
                new Order()
                {
                    Id = "1",
                    ReservationId = 3,
                    Reservation = new Reservation{ UserId = "123"},
                    Status = OrderStatus.Processing,
                    Date = DateTime.UtcNow,
                },
                new Order()
                {
                    Id = "2",
                    ReservationId = 1,
                    Reservation = new Reservation{ UserId = "456"},
                    Status = OrderStatus.Reserved,
                    Date = DateTime.UtcNow,
                }
            };

        public static List<OrderDetail> OrderDetails
            => new List<OrderDetail>()
            {
                new OrderDetail()
                {
                    Id = 1,
                    FoodId= 1,
                    Price= 10000,
                    Note= "test",
                    Status = OrderDetailStatus.Served,
                    OrderId = "1",
                    Created = DateTime.UtcNow
                },
                new OrderDetail()
                {
                    Id = 2,
                    FoodId= 2,
                    Price= 20000,
                    Note= "test",
                    Status = OrderDetailStatus.Processing,
                    OrderId = "2",
                    Created = DateTime.UtcNow.AddHours(-1)
                },
                new OrderDetail()
                {
                    Id = 3,
                    FoodId= 1,
                    Price= 10000,
                    Note= "test",
                    Status = OrderDetailStatus.Served,
                    OrderId = "1",
                    Created = DateTime.UtcNow
                },
            };

        public static List<ApplicationUser> Users
            => new List<ApplicationUser>()
            {
                new ApplicationUser()
                {
                    Id = "123",
                    UserName= "defaultCustomer",
                    FullName = "Name",
                    PhoneNumber= "1234567890",
                },
                new ApplicationUser()
                {
                    Id = "456",
                    UserName= "Random",
                    FullName = "Name Random",
                    PhoneNumber= "0987654321",
                }
            };
    }
}
