using Application.Common.Mappings;
using Core.Common;
using Core.Common.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public sealed class TableRepository : GenericRepository<Table>, ITableRepository
    {
        public TableRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.Tables;
        }

        public async Task<List<Table>> GetTableOnNumOfSeatAndType(int NumOfSeat, TableType type)
        {
            IQueryable<Table> query = _dbSet;


            query = query.Where(t => t.Type == type && t.NumOfSeats == NumOfSeat);

            return await query.ToListAsync();
        }
    }
}
