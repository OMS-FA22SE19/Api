using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class TableRepository : GenericRepository<Table>, ITableRepository
    {
        public TableRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.Tables;
        }
    }
}
