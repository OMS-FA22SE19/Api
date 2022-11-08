using Core.Common.Interfaces;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Repositories
{
    public class ReservationTableRepository : EntityRepository<ReservationTable>, IReservationTableRepository
    {
        public ReservationTableRepository(IApplicationDbContext context) : base(context, context.ReservationTables)
        {
        }
    }
}
