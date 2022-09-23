using Core.Common.Interfaces;
using Core.Interfaces;
using Infrastructure.Repositories;

namespace Infrastructure.Common
{
    public sealed class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IApplicationDbContext _context;
        private ITableRepository _tableRepository;
        private IReservationRepository _reservationRepository;
        private ICategoryRepository _categoryRepository;
        private IFoodRepository _foodRepository;
        private IFoodCategoryRepository _foodCategoryRepository;
        private IMenuRepository _menuRepository;
        private IMenuFoodRepository _menuFoodRepository;
        private IOrderRepository _orderRepository;
        private IOrderDetailRepository _orderDetailRepository;

        public UnitOfWork(IApplicationDbContext context)
        {
            _context = context;
        }
        public ITableRepository TableRepository
        {
            get
            {
                if (_tableRepository is null)
                {
                    _tableRepository = new TableRepository(_context);
                }
                return _tableRepository;
            }
        }
        public IReservationRepository ReservationRepository
        {
            get
            {
                if (_reservationRepository is null)
                {
                    _reservationRepository = new ReservationRepository(_context);
                }
                return _reservationRepository;
            }
        }
        public IFoodRepository FoodRepository
        {
            get
            {
                if (_foodRepository is null)
                {
                    _foodRepository = new FoodRepository(_context);
                }
                return _foodRepository;
            }
        }
        public ICategoryRepository CategoryRepository
        {
            get
            {
                if (_categoryRepository is null)
                {
                    _categoryRepository = new CategoryRepository(_context);
                }
                return _categoryRepository;
            }
        }
        public IFoodCategoryRepository FoodCategoryRepository
        {
            get
            {
                if (_foodCategoryRepository is null)
                {
                    _foodCategoryRepository = new FoodCategoryRepository(_context);
                }
                return _foodCategoryRepository;
            }
        }

        public IMenuRepository MenuRepository
        {
            get
            {
                if (_menuRepository is null)
                {
                    _menuRepository = new MenuRepository(_context);
                }
                return _menuRepository;
            }
        }
        public IMenuFoodRepository MenuFoodRepository
        {
            get
            {
                if (_menuFoodRepository is null)
                {
                    _menuFoodRepository = new MenuFoodRepository(_context);
                }
                return _menuFoodRepository;
            }
        }

        public IOrderRepository OrderRepository
        {
            get
            {
                if (_orderRepository is null)
                {
                    _orderRepository = new OrderRepository(_context);
                }
                return _orderRepository;
            }
        }

        public IOrderDetailRepository OrderDetailRepository
        {
            get
            {
                if (_orderDetailRepository is null)
                {
                    _orderDetailRepository = new OrderDetailRepository(_context);
                }
                return _orderDetailRepository;
            }
        }

        public async Task CompleteAsync(CancellationToken cancellationToken) => await _context.SaveChangesAsync(cancellationToken);
        public void Dispose() => _context.Dispose();
    }
}
