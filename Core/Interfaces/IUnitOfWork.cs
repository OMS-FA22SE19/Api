namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
        IRefreshTokenRepository RefreshTokenRepository { get; }
        IAdminSettingRepository AdminSettingRepository { get; }
        ITopicRepository TopicRepository { get; }
        IUserTopicRepository UserTopicRepository { get; }
        IUserDeviceTokenRepository UserDeviceTokenRepository { get; }
        IUserRepository UserRepository { get; }
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
        IBillingRepository BillingRepository { get; }
        ITableTypeRepository TableTypeRepository { get; }
        IReservationTableRepository ReservationTableRepository { get; }
        Task CompleteAsync(CancellationToken cancellationToken);
    }
}
