namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        public ITableRepository TableRepository { get; }
        public IReservationRepository ReservationRepository { get; }
        public IFoodRepository FoodRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        Task CompleteAsync(CancellationToken cancellationToken);
    }
}
