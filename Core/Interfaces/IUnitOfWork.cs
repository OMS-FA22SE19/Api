namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        ITableRepository TableRepository { get; }
        IReservationRepository ReservationRepository { get; }
        IFoodRepository FoodRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IFoodCategoryRepository FoodCategoryRepository { get; }
        Task CompleteAsync(CancellationToken cancellationToken);
    }
}
