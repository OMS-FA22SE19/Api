using Core.Entities;

namespace Core.Interfaces
{
    public interface ITableRepository : IGenericRepository<Table>
    {
        Task<int> GetClosestNumOfSeatTable(int NumOfPeople);
        Task<int> GetTableAvailableForReservation(List<int> tableIds, DateTime StartTime, DateTime EndTime);
        Task<List<Table>> GetTableOnNumOfSeatAndType(int NumOfSeat, int tableTypeId);
        Task<List<Table>> GetTableWithSeatsNumber(int NumOfSeats);
        Task<List<Table>> GetAllAvailableTableWithDateAndTableType(DateTime startTime, DateTime endTime, int tableTypeId, int numOfPeople);
    }
}
