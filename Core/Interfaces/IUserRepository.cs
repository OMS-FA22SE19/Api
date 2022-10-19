using Core.Common;
using Core.Entities;
using System.Linq.Expressions;

namespace Core.Interfaces
{
    public interface IUserRepository
    {
        //Task<bool> DeleteAsync(IList<ApplicationUser> entityToDelete);
        //Task<bool> DeleteAsync(Expression<Func<ApplicationUser, bool>> expression);
        Task<bool> DeleteAsync(ApplicationUser entity);
        Task<List<ApplicationUser>> GetAllAsync(
            List<Expression<Func<ApplicationUser, bool>>> filters = null,
        Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>> orderBy = null,
            string includeProperties = "");
        Task<PaginatedList<ApplicationUser>> GetPaginatedListAsync(
            List<Expression<Func<ApplicationUser, bool>>> filter = null,
        Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>> orderBy = null,
            string includeProperties = "",
            int pageIndex = 1,
            int pageSize = 50);
        Task<ApplicationUser> GetAsync(Expression<Func<ApplicationUser, bool>> expression, string includeProperties = "");
        Task<ApplicationUser> InsertAsync(ApplicationUser entity);
        Task<ApplicationUser> UpdateAsync(ApplicationUser entityToUpdate);
    }
}
