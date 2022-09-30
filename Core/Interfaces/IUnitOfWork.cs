namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        ITableRepository TableRepository { get; }
        IReservationRepository ReservationRepository { get; }
        IFoodRepository FoodRepository { get; }
        ITypeRepository TypeRepository { get; }
        ICourseTypeRepository CourseTypeRepository { get; }
        IFoodTypeRepository FoodTypeRepository { get; }
        IMenuRepository MenuRepository { get; }
        IMenuFoodRepository MenuFoodRepository { get; }
        IOrderRepository OrderRepository { get; }
        IOrderDetailRepository OrderDetailRepository { get; }
        ITableTypeRepository TableTypeRepository { get; }
        Task CompleteAsync(CancellationToken cancellationToken);
    }
}
