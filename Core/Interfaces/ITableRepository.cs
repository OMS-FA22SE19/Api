using Core.Entities;
using Core.Enums;

namespace Core.Interfaces
{
    public interface ITableRepository : IGenericRepository<Table>
    {
        Task<List<Table>> GetTableOnNumOfSeatAndType(int NumOfSeat, TableType type);
    }
}
