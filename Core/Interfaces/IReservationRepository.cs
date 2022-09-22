using Core.Entities;

namespace Core.Interfaces
{
    public interface IReservationRepository : IGenericRepository<Reservation>
    {
        Task<List<Reservation>> GetAllReservationWithDate(DateTime date);
        Task<int> GetTableAvailableForReservation(List<int> tableIds, DateTime StartTime, DateTime EndTime);
    }
}
