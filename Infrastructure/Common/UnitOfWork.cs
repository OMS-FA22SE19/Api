using Core.Common.Interfaces;
using Core.Interfaces;
using Infrastructure.Repositories;

namespace Infrastructure.Common
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IApplicationDbContext _context;
        private ITableRepository _tableRepository;

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
        public async Task CompleteAsync(CancellationToken cancellationToken) => await _context.SaveChangesAsync(cancellationToken);
        public void Dispose() => _context.Dispose();
    }
}
