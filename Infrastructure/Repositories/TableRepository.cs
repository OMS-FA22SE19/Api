using Core.Common.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class TableRepository : AuditableEntityRepository<Table>, ITableRepository
    {
        private IQueryable<Reservation> _dbSetReservation;
        private IQueryable<ReservationTable> _dbSetReservationTable;
        public TableRepository(IApplicationDbContext context) : base(context, context.Tables)
        {
            _dbSetReservation = context.Reservations;
            _dbSetReservationTable = context.ReservationTables;
        }

        public async Task<List<Table>> GetTableOnNumOfSeatAndType(int NumOfSeat, int? tableTypeId)
        {
            IQueryable<Table> query = _dbSet;

            if (tableTypeId is not null)
            {
                query = query.Where(t => t.TableTypeId == tableTypeId);
            }
            query = query.Where(t => t.NumOfSeats == NumOfSeat && t.IsDeleted == false);

            return await query.ToListAsync();
        }

        public async Task<int> GetClosestNumOfSeatTable(int NumOfPeople)
        {
            IQueryable<Table> query = _dbSet;
            int NumOfSeats;
            query = query.Where(t => t.NumOfSeats >= NumOfPeople && !t.IsDeleted).OrderBy(t => t.NumOfSeats);
            if (query.Any())
            {
                Table? FirstTable = await query.FirstOrDefaultAsync();
                NumOfSeats = FirstTable.NumOfSeats;
            }
            else return 0;

            return NumOfSeats;
        }

        public async Task<List<Table>> GetTableWithSeatsNumber(int NumOfSeats)
        {
            IQueryable<Table> query = _dbSet;
            string includeStrings = $"{nameof(Table.TableType)}";
            query = query.Where(t => t.NumOfSeats == NumOfSeats && !t.IsDeleted);

            return await query.Include(includeStrings).ToListAsync();
        }

        public async Task<int> GetTableAvailableForReservation(List<int> tableIds, DateTime StartTime, DateTime EndTime)
        {
            IQueryable<Reservation> query = _dbSetReservation;
            IQueryable<ReservationTable> queryReservationTable = _dbSetReservationTable;

            queryReservationTable = queryReservationTable.Where(rt => tableIds.Any(tableId => tableId.Equals(rt.TableId)));
            List<int> reservationTableList = queryReservationTable.Select(rt => rt.ReservationId).ToList();

            query = query.Where(r => reservationTableList.Any(reservationTable => reservationTable.Equals(r.Id))
                 && !((StartTime < r.StartTime && EndTime <= r.StartTime) || (StartTime >= r.EndTime && EndTime > r.EndTime))
                 && r.Status != ReservationStatus.Available)
                .OrderBy(r => r.StartTime);

            List<int> ReservationIds = query.Select(r => r.Id).ToList();

            List<int> notAvailableTable = queryReservationTable
                .Where(rt => ReservationIds.Any(ReservationId => ReservationId.Equals(rt.ReservationId)))
                .Select(rt => rt.TableId).Distinct().ToList();
            int AvailableTable = tableIds.Except(notAvailableTable).FirstOrDefault();

            return AvailableTable;
        }

        public async Task<List<Table>> GetAllAvailableTableWithDateAndTableType(DateTime startTime, DateTime endTime, int tableTypeId, int numOfPeople)
        {
            IQueryable<Table> query = _dbSet;
            IQueryable<Reservation> queryReservation = _dbSetReservation;
            IQueryable<ReservationTable> queryReservationTable = _dbSetReservationTable;


            List<Table> listTable = await query.Where(t => t.TableTypeId == tableTypeId && t.NumOfSeats <= numOfPeople + 2 && t.IsDeleted == false)
                .Include(t => t.TableType)
                .OrderBy(t => t.NumOfSeats)
                .ToListAsync();

            queryReservation = queryReservation.Where(r =>
            !((startTime < r.StartTime && endTime <= r.StartTime) || (startTime >= r.EndTime && endTime > r.EndTime))
                && r.ReservationTables.Any(ReservationTable => ReservationTable.Table.TableTypeId == tableTypeId));

            List<int> ReservationIds = queryReservation.Select(r => r.Id).ToList();
            List<int> listOfTableIdNotAvailable = queryReservationTable
                .Where(rt => ReservationIds.Any(ReservationId => ReservationId.Equals(rt.ReservationId)))
                .Select(rt => rt.TableId).Distinct().ToList();
            listTable.RemoveAll(item => listOfTableIdNotAvailable.Contains(item.Id));

            return listTable;
        }
    }
}
