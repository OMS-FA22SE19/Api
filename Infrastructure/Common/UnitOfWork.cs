﻿using Core.Common.Interfaces;
using Core.Interfaces;
using Infrastructure.Repositories;

namespace Infrastructure.Common
{
    public sealed class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IApplicationDbContext _context;
        private IRefreshTokenRepository _refreshTokenRepository;
        private IAdminSettingRepository _adminSettingRepository;
        private ITopicRepository _topicRepository;
        private IUserTopicRepository _userTopicRepository;
        private IUserDeviceTokenRepository _userDeviceTokenRepository;
        private IUserRepository _userRepository;
        private ITableRepository _tableRepository;
        private IReservationRepository _reservationRepository;
        private ITypeRepository _typeRepository;
        private ICourseTypeRepository _courseTypeRepository;
        private IFoodRepository _foodRepository;
        private IFoodTypeRepository _foodTypeRepository;
        private IMenuRepository _menuRepository;
        private IMenuFoodRepository _menuFoodRepository;
        private IOrderRepository _orderRepository;
        private IOrderDetailRepository _orderDetailRepository;
        private IBillingRepository _billingRepository;
        private ITableTypeRepository _tableTypeRepository;
        private IReservationTableRepository _reservationTableRepository;

        public UnitOfWork(IApplicationDbContext context)
        {
            _context = context;
        }

        public IRefreshTokenRepository RefreshTokenRepository
        {
            get
            {
                if (_refreshTokenRepository is null)
                {
                    _refreshTokenRepository = new RefreshTokenRepository(_context);
                }
                return _refreshTokenRepository;
            }
        }
        public IAdminSettingRepository AdminSettingRepository
        {
            get
            {
                if (_adminSettingRepository is null)
                {
                    _adminSettingRepository = new AdminSettingRepository(_context);
                }
                return _adminSettingRepository;
            }
        }
        public ITopicRepository TopicRepository
        {
            get
            {
                if (_topicRepository is null)
                {
                    _topicRepository = new TopicRepository(_context);
                }
                return _topicRepository;
            }
        }
        public IUserTopicRepository UserTopicRepository
        {
            get
            {
                if (_userTopicRepository is null)
                {
                    _userTopicRepository = new UserTopicRepository(_context);
                }
                return _userTopicRepository;
            }
        }
        public IUserDeviceTokenRepository UserDeviceTokenRepository
        {
            get
            {
                if (_userDeviceTokenRepository is null)
                {
                    _userDeviceTokenRepository = new UserDeviceTokenRepository(_context);
                }
                return _userDeviceTokenRepository;
            }
        }
        public IUserRepository UserRepository
        {
            get
            {
                if (_userRepository is null)
                {
                    _userRepository = new UserRepository(_context);
                }
                return _userRepository;
            }
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
        public ITypeRepository TypeRepository
        {
            get
            {
                if (_typeRepository is null)
                {
                    _typeRepository = new TypeRepository(_context);
                }
                return _typeRepository;
            }
        }

        public ICourseTypeRepository CourseTypeRepository
        {
            get
            {
                if (_courseTypeRepository is null)
                {
                    _courseTypeRepository = new CourseTypeRepository(_context);
                }
                return _courseTypeRepository;
            }
        }

        public IFoodTypeRepository FoodTypeRepository
        {
            get
            {
                if (_foodTypeRepository is null)
                {
                    _foodTypeRepository = new FoodTypeRepository(_context);
                }
                return _foodTypeRepository;
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

        public IBillingRepository BillingRepository
        {
            get
            {
                if (_billingRepository is null)
                {
                    _billingRepository = new BillingRepository(_context);
                }
                return _billingRepository;
            }
        }

        public ITableTypeRepository TableTypeRepository
        {
            get
            {
                if (_tableTypeRepository is null)
                {
                    _tableTypeRepository = new TableTypeRepository(_context);
                }
                return _tableTypeRepository;
            }
        }

        public IReservationTableRepository ReservationTableRepository
        {
            get
            {
                if (_reservationTableRepository is null)
                {
                    _reservationTableRepository = new ReservationTableRepository(_context);
                }
                return _reservationTableRepository;
            }
        }

        public async Task CompleteAsync(CancellationToken cancellationToken) => await _context.SaveChangesAsync(cancellationToken);
        public void Dispose() => _context.Dispose();
    }
}
