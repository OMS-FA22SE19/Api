using Application.Common.Mappings;
using Core.Common;
using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;
using Firebase.Auth;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        internal IApplicationDbContext _context;
        internal DbSet<ApplicationUser> _dbSet;
        public UserRepository(IApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Users;
        }

        public async Task<PaginatedList<ApplicationUser>> GetPaginatedListAsync(
            List<Expression<Func<ApplicationUser, bool>>> filters = null,
            Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>> orderBy = null,
            string includeProperties = "",
            int pageIndex = 1,
            int pageSize = 50)
        {
            IQueryable<ApplicationUser> query = _dbSet.AsNoTracking();


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

        public async Task<List<ApplicationUser>> GetAllAsync(
            List<Expression<Func<ApplicationUser, bool>>> filters = null,
            Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>> orderBy = null,
            string includeProperties = "")
        {
            IQueryable<ApplicationUser> query = _dbSet.AsNoTracking();


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
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public async Task<ApplicationUser> GetAsync(Expression<Func<ApplicationUser, bool>> expression, string includeProperties = "")
        {
            IQueryable<ApplicationUser> query = _dbSet.AsNoTracking();
            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            return await query.FirstOrDefaultAsync(expression);
        }

        public async Task<ApplicationUser> InsertAsync(ApplicationUser entity)
        {
            _dbSet.Add(entity);
            return await Task.FromResult(entity);
        }

        public async Task<bool> DeleteAsync(ApplicationUser entity)
        {
            entity.IsDeleted = true;
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            return await Task.FromResult(true);
        }

        public async Task<ApplicationUser> UpdateAsync(ApplicationUser entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;
            return await Task.FromResult(entityToUpdate);
        }
    }
}
