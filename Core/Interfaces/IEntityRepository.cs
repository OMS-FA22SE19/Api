using Core.Common;
using Domain.Common;
using System.Linq.Expressions;

namespace Core.Interfaces
{
    public interface IEntityRepository<TEntity> where TEntity : Entity
    {
        Task<bool> DeleteAsync(IList<TEntity> entitiesToDelete);
        Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> expression);
        Task<List<TEntity>> GetAllAsync(
            List<Expression<Func<TEntity, bool>>> filters = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");
        Task<PaginatedList<TEntity>> GetPaginatedListAsync(
            List<Expression<Func<TEntity, bool>>> filters = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "",
            int pageIndex = 1,
            int pageSize = 50);
        Task<TEntity> GetAsync(List<Expression<Func<TEntity, bool>>> filters = null, string includeProperties = "");
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression, string includeProperties = "");
        Task<TEntity> InsertAsync(TEntity entity);
        Task<IList<TEntity>> InsertAsync(IList<TEntity> entities);
        Task<TEntity> UpdateAsync(TEntity entityToUpdate);
    }

    public interface IAuditableEntityRepository<TEntity> : IEntityRepository<TEntity> where TEntity : BaseAuditableEntity
    {
        Task<TEntity> RestoreAsync(TEntity entityToRecover);
    }
}