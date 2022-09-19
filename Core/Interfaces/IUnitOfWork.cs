namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        ITableRepository TableRepository { get; }
        IFoodRepository FoodRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IFoodCategoryRepository FoodCategoryRepository { get; }
        public ITableRepository TableRepository { get; }
        public IReservationRepository ReservationRepository { get; }
        public IFoodRepository FoodRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        Task CompleteAsync(CancellationToken cancellationToken);
    }
}
