namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        ITableRepository TableRepository { get; }
        public IFoodRepository FoodRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        Task CompleteAsync(CancellationToken cancellationToken);
    }
}
