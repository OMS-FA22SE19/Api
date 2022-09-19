﻿using Core.Common;
using System.Linq.Expressions;

namespace Core.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : Entity
    {
        Task<bool> DeleteAsync(IList<TEntity> entityToDelete);
        Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> expression);
        Task<PaginatedList<TEntity>> GetPaginatedListAsync(
            List<Expression<Func<TEntity, bool>>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "",
            int pageIndex = 1,
            int pageSize = 50);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression, string includeProperties = "");
        Task<TEntity> InsertAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entityToUpdate);
    }
}