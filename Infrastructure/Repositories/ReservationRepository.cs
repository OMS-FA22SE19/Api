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
    }
}
