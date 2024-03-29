﻿using Application.Common.Mappings;
using Core.Common;
using Core.Common.Interfaces;
using Core.Interfaces;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class EntityRepository<TEntity> : IEntityRepository<TEntity> where TEntity : Entity
    {
        internal IApplicationDbContext _context;
        internal DbSet<TEntity> _dbSet;

        public EntityRepository(IApplicationDbContext context, DbSet<TEntity> dbSet)
        {
            _context = context;
            _dbSet = dbSet;
        }

        public virtual async Task<PaginatedList<TEntity>> GetPaginatedListAsync(
            List<Expression<Func<TEntity, bool>>> filters = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "",
            int pageIndex = 1,
            int pageSize = 50)
        {
            IQueryable<TEntity> query = _dbSet.AsNoTracking();


            if (filters is not null && filters.Any())
            {
                foreach (var filter in filters)
                {
                    query = query.Where(filter);
                }
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                return await orderBy(query).PaginatedListAsync(pageIndex, pageSize);
            }
            else
            {
                return await query.PaginatedListAsync(pageIndex, pageSize);
            }
        }

        public virtual async Task<List<TEntity>> GetAllAsync(
            List<Expression<Func<TEntity, bool>>> filters = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = _dbSet.AsNoTracking();


            if (filters is not null && filters.Any())
            {
                foreach (var filter in filters)
                {
                    query = query.Where(filter);
                }
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public virtual async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression, string includeProperties = "")
        {
            IQueryable<TEntity> query = _dbSet.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }
            return await query.FirstOrDefaultAsync(expression);
        }

        public virtual async Task<TEntity> GetAsync(List<Expression<Func<TEntity, bool>>> filters = null, string includeProperties = "")
        {
            IQueryable<TEntity> query = _dbSet.AsNoTracking();
            if (filters is not null && filters.Any())
            {
                foreach (var filter in filters)
                {
                    query = query.Where(filter);
                }
            }
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }
            return await query.FirstOrDefaultAsync();
        }

        public virtual async Task<TEntity> InsertAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            return await Task.FromResult(entity);
        }

        public virtual async Task<IList<TEntity>> InsertAsync(IList<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            return await Task.FromResult(entities);
        }

        public virtual async Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> expression)
        {
            var entitiesToDelete = await _dbSet.Where(expression).ToListAsync();
            if (entitiesToDelete != null)
            {
                await DeleteAsync(entitiesToDelete);
            }
            return await Task.FromResult(true);
        }

        public virtual async Task<bool> DeleteAsync(IList<TEntity> entitiesToDelete)
        {
            foreach (TEntity entity in entitiesToDelete)
            {
                if (_context.Entry(entity).State == EntityState.Detached)
                {
                    _dbSet.Attach(entity);
                }
            }
            _dbSet.RemoveRange(entitiesToDelete);
            return await Task.FromResult(true);
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;
            return await Task.FromResult(entityToUpdate);
        }
    }

    public class AuditableEntityRepository<TEntity> : EntityRepository<TEntity>, IAuditableEntityRepository<TEntity> where TEntity : BaseAuditableEntity
    {
        public AuditableEntityRepository(IApplicationDbContext context, DbSet<TEntity> dbSet) : base(context, dbSet)
        {
            _context = context;
            _dbSet = dbSet;
        }

        public virtual async Task<TEntity> RestoreAsync(TEntity entityToRecover)
        {
            if (entityToRecover.IsDeleted)
            {
                entityToRecover.IsDeleted = false;
            }
            _dbSet.Attach(entityToRecover);
            _context.Entry(entityToRecover).State = EntityState.Modified;
            return await Task.FromResult(entityToRecover);
        }
    }
}
