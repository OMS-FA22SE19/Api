using Core.Common.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ReservationRepository : GenericRepository<Reservation>, IReservationRepository
    {
        private IQueryable<ReservationTable> _dbSetReservationTable;
        public ReservationRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.Reservations;
            _dbSetReservationTable = context.ReservationTables;
        }

        public async Task<List<Reservation>> GetAllReservationWithDate(DateTime date, int? tableTypeId, int? numOfSeats)
        {
            IQueryable<Reservation> query = _dbSet;

            query = query.Where(r =>
            r.StartTime.Date == date.Date
            && r.EndTime.Date == date.Date
            && r.Status != ReservationStatus.Available
            && !r.IsDeleted).OrderBy(r => r.StartTime)
            .Include(r => r.ReservationTables).ThenInclude(r => r.Table);

            if (tableTypeId is not null)
            {
                query = query.Where(e => e.TableTypeId == tableTypeId);
                if (numOfSeats is not null)
                {
                    query = query.Where(e => e.NumOfSeats == numOfSeats);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<Reservation> GetReservationWithDateAndTableId(int tableId, DateTime date)
        {
            IQueryable<Reservation> query = _dbSet;
            IQueryable<ReservationTable> queryReservationTable = _dbSetReservationTable;

            queryReservationTable = queryReservationTable.Where(rt => rt.TableId == tableId);
            List<int> reservationTableList = queryReservationTable.Select(rt => rt.ReservationId).ToList();

            return await query.OrderByDescending(e => e.StartTime)
                .FirstOrDefaultAsync(e => reservationTableList.Contains(e.Id)
                && e.StartTime < date.AddHours(2)
                && e.StartTime > date.AddHours(-2));
        }
    }
}
