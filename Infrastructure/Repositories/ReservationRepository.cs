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
                && !((StartTime < r.StartTime && EndTime < r.StartTime) || (StartTime > r.EndTime && EndTime > r.EndTime))
                 && (r.StartTime > StartTime.Date && r.StartTime < StartTime.Date.AddDays(1))
                 && r.Status != ReservationStatus.Available)
                .OrderBy(r => r.StartTime);

            List<int> notAvailableTable = query.Select(e=>e.TableId).ToList();
            int AvailableTable = tableIds.Except(notAvailableTable).FirstOrDefault();

            Console.WriteLine(AvailableTable);

            return AvailableTable;
        }
    }
}
