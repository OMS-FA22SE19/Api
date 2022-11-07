using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Core.Common.Interfaces
{
    public interface IApplicationDbContext : IDisposable
    {
        DbSet<AdminSetting> AdminSettings { get; }
        DbSet<Topic> Topics { get; }
        DbSet<UserTopic> UserTopics { get; }
        DbSet<UserDeviceToken> UserDeviceTokens { get; }
        DbSet<ApplicationUser> Users { get; }
        DbSet<Entities.Type> Types { get; }
        DbSet<Food> Foods { get; }
        DbSet<FoodType> FoodTypes { get; }
        DbSet<CourseType> CourseTypes { get; }
        DbSet<Menu> Menus { get; }
        DbSet<MenuFood> MenuFoods { get; }
        DbSet<Order> Orders { get; }
        DbSet<OrderDetail> OrderDetails { get; }
        DbSet<Billing> Billings { get; }
        DbSet<Reservation> Reservations { get; }
        DbSet<ReservationTable> ReservationTables { get; }
        DbSet<Table> Tables { get; }
        DbSet<TableType> TableTypes { get; }

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
