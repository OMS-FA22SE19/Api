using Core.Common.Interfaces;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public sealed class TypeRepository : AuditableEntityRepository<Core.Entities.Type>, ITypeRepository
    {
        public TypeRepository(IApplicationDbContext context) : base(context, context.Types)
        {
        }
    }
}
