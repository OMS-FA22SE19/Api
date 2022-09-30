﻿using Core.Common.Interfaces;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class TypeRepository : GenericRepository<Core.Entities.Type>, ITypeRepository
    {
        public TypeRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.Types;
        }
    }
}
