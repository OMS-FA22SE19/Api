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

            query = query.Where(r =>
            r.StartTime >= date.Date && r.StartTime < date.Date.AddDays(1)
            && r.EndTime >= date.Date && r.EndTime < date.Date.AddDays(1)
            && r.Status != ReservationStatus.Available).OrderBy(r => r.StartTime);

            return await query.ToListAsync();
        }

        public async Task<Reservation> GetReservationWithDateAndTableId(int tableId, DateTime date)
        {
            IQueryable<Reservation> query = _dbSet;
            return await query.OrderByDescending(e => e.StartTime).FirstOrDefaultAsync(e => e.TableId == tableId && e.StartTime < date.AddHours(1) && e.StartTime > date.AddHours(-1));
        }
    }
}
