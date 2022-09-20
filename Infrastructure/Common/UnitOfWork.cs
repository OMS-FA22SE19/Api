using Core.Common.Interfaces;
using Core.Interfaces;
using Infrastructure.Repositories;

namespace Infrastructure.Common
{
    public sealed class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IApplicationDbContext _context;
        private ITableRepository _tableRepository;
        private ICategoryRepository _categoryRepository;
        private IFoodRepository _foodRepository;
        private IFoodCategoryRepository _foodCategoryRepository;

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

        public async Task CompleteAsync(CancellationToken cancellationToken) => await _context.SaveChangesAsync(cancellationToken);
        public void Dispose() => _context.Dispose();
    }
}
