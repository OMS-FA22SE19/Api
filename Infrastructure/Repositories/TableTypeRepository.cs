using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class TableTypeRepository : AuditableEntityRepository<TableType>, ITableTypeRepository
    {
        public TableTypeRepository(IApplicationDbContext context) : base(context, context.TableTypes)
        {
        }
    }
}
