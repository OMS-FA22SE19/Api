using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class ReservationTableRepository : GenericRepository<ReservationTable>, IReservationTableRepository
    {
        public ReservationTableRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.ReservationTables;
        }
    }
}
