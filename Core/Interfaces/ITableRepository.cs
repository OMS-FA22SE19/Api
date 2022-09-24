using Core.Entities;
using Core.Enums;

namespace Core.Interfaces
{
    public interface ITableRepository : IGenericRepository<Table>
    {
        Task<int> GetClosestNumOfSeatTable(int NumOfPeople);
        Task<int> GetTableAvailableForReservation(List<int> tableIds, DateTime StartTime, DateTime EndTime);
        Task<List<Table>> GetTableOnNumOfSeatAndType(int NumOfSeat, TableType type);
        Task<List<Table>> GetTableWithSeatsNumber(int NumOfSeats);
    }
}
