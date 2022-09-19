using Application.Common.Mappings;
using Core.Common;
using Core.Common.Interfaces;
using Core.Interfaces;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        internal IApplicationDbContext _context;
        internal DbSet<TEntity> _dbSet;
        public GenericRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public virtual async Task<PaginatedList<TEntity>> GetPaginatedListAsync(
            List<Expression<Func<TEntity, bool>>> filters = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "",
            int pageIndex = 1,
            int pageSize = 50)
        {
            IQueryable<TEntity> query = _dbSet;


            if (filters is not null && filters.Any())
            {
                foreach (var filter in filters)
                {
                    query = query.Where(filter);
                }
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return await orderBy(query).PaginatedListAsync(pageIndex, pageSize);
            }
            else
            {
                return await query.PaginatedListAsync(pageIndex, pageSize); ;
            }
        }

        public virtual async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression)
            => await _dbSet.FirstOrDefaultAsync(expression);

        public virtual async Task<TEntity> InsertAsync(TEntity entity)
        {
            _dbSet.Add(entity);
            return await Task.FromResult(entity);
        }

        public virtual async Task<bool> DeleteAsync(object id)
        {
            TEntity entityToDelete = await _dbSet.FindAsync(id);
            if (entityToDelete != null)
            {
                await DeleteAsync(entityToDelete);
            }
            return await Task.FromResult(true);
        }

        public virtual async Task<bool> DeleteAsync(TEntity entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }
            _dbSet.Remove(entityToDelete);
            return await Task.FromResult(true);
        }

        public virtual Task UpdateAsync(TEntity entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;
            return Task.FromResult(entityToUpdate);
        }
    }
}
