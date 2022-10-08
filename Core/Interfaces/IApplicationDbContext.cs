using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Core.Common.Interfaces
{
    public interface IApplicationDbContext : IDisposable
    {
        DbSet<Entities.Type> Types { get; }
        DbSet<Food> Foods { get; }
        DbSet<FoodType> FoodTypes { get; }
        DbSet<CourseType> CourseTypes { get; }
        DbSet<Menu> Menus { get; }
        DbSet<MenuFood> MenuFoods { get; }
        DbSet<Order> Orders { get; }
        DbSet<OrderDetail> OrderDetails { get; }
        DbSet<Payment> Payments { get; }
        DbSet<Reservation> Reservations { get; }
        DbSet<Table> Tables { get; }
        DbSet<TableType> TableTypes { get; }

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
