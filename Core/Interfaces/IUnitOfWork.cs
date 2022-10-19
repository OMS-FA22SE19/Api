﻿namespace Core.Interfaces
{
    public interface IUnitOfWork
    {
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
        IPaymentRepository PaymentRepository { get; }
        ITableTypeRepository TableTypeRepository { get; }
        IReservationTableRepository ReservationTableRepository { get; }
        Task CompleteAsync(CancellationToken cancellationToken);
    }
}
