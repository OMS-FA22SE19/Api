using Core.Common.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public sealed class TableRepository : GenericRepository<Table>, ITableRepository
    {
        private IQueryable<Reservation> _dbSetReservation;

        public TableRepository(IApplicationDbContext context) : base(context)
        {
            _dbSet = context.Tables;
            _dbSetReservation = context.Reservations;
        }

        public async Task<List<Table>> GetTableOnNumOfSeatAndType(int NumOfSeat, int tableTypeId)
        {
            IQueryable<Table> query = _dbSet;


            query = query.Where(t => t.TableTypeId == tableTypeId && t.NumOfSeats == NumOfSeat && t.IsDeleted == false);

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

            query = query.Where(r => tableIds.Any(tableId => tableId.Equals(r.TableId))
                 && !((StartTime < r.StartTime && EndTime <= r.StartTime) || (StartTime >= r.EndTime && EndTime > r.EndTime))
                 && r.Status != ReservationStatus.Available)
                .OrderBy(r => r.StartTime);

            List<int> notAvailableTable = query.Select(e => e.TableId).ToList();
            int AvailableTable = tableIds.Except(notAvailableTable).FirstOrDefault();

            return AvailableTable;
        }

        public async Task<List<Table>> GetAllAvailableTableWithDateAndTableType(DateTime startTime, DateTime endTime, int tableTypeId, int numOfPeople)
        {
            IQueryable<Table> query = _dbSet;
            IQueryable<Reservation> queryReservation = _dbSetReservation;

            List<Table> listTable = await query.Where(t => t.TableTypeId == tableTypeId && t.NumOfSeats <= numOfPeople + 2 && t.IsDeleted == false)
                .Include(t => t.TableType)
                .OrderBy(t => t.NumOfSeats)
                .ToListAsync();

            queryReservation = queryReservation.Where(r => 
            !((startTime < r.StartTime && endTime <= r.StartTime) || (startTime >= r.EndTime && endTime > r.EndTime))
                && r.Table.TableTypeId == tableTypeId);

            List<int> listOfTableIdNotAvailable = queryReservation.Select(e => e.TableId).ToList();
            listTable.RemoveAll(item => listOfTableIdNotAvailable.Contains(item.Id));

            return listTable;
        }
    }
}
