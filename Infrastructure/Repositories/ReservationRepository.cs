using Core.Common.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
    {
        public ReservationRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.Reservations;
        }

        public async Task<List<Reservation>> GetAllReservationWithDate(DateTime date)
        {
            IQueryable<Reservation> query = _dbSet;

            query = query.Where(r => r.StartTime >= date && r.StartTime < date.AddDays(1)).OrderBy(r => r.StartTime);

            return await query.ToListAsync();
        }

        public async Task<int> GetTableAvailableForReservation(List<int> tableIds, DateTime StartTime, DateTime EndTime)
        {
            IQueryable<Reservation> query = _dbSet;

            query = query.Where(r => tableIds.Any(tableId => tableId.Equals(r.TableId)) 
                && ((StartTime < r.StartTime && EndTime < r.StartTime) || (StartTime > r.StartTime && EndTime > r.StartTime)))
                .OrderBy(r => r.StartTime);

            int table = query.FirstOrDefault().TableId;

            return table;
        }
    }
}
