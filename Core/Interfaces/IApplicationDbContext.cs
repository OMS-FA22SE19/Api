using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Core.Common.Interfaces
{
    public interface IApplicationDbContext : IDisposable
    {
        DbSet<Category> Categories { get; }
        DbSet<Food> Foods { get; }
        //DbSet<FoodCategory> FoodCategories { get; }
        DbSet<Menu> Menus { get; }
        //DbSet<MenuFood> MenuFoods { get; }
        DbSet<Order> Orders { get; }
        DbSet<OrderDetail> OrderDetails { get; }
        DbSet<Reservation> Reservations { get; }
        DbSet<Table> Tables { get; }

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
