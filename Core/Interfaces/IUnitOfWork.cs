namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        ITableRepository TableRepository { get; }
        IReservationRepository ReservationRepository { get; }
        IFoodRepository FoodRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IFoodCategoryRepository FoodCategoryRepository { get; }
        IMenuRepository MenuRepository { get; }
        IMenuFoodRepository MenuFoodRepository { get; }
        IOrderRepository OrderRepository { get; }
        public IOrderDetailRepository OrderDetailRepository { get; }
        Task CompleteAsync(CancellationToken cancellationToken);
    }
}
