﻿using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class FoodRepository : GenericRepository<Food>, IFoodRepository
    {
        public FoodRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.Foods;
        }
    }
}
