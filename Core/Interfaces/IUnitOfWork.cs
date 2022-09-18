namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        ITableRepository TableRepository { get; }
        Task CompleteAsync(CancellationToken cancellationToken);
    }
}
