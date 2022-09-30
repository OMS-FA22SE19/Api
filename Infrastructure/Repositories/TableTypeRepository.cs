﻿using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class TableTypeRepository : GenericRepository<TableType>, ITableTypeRepository
    {
        public TableTypeRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.TableTypes;
        }
    }
}
