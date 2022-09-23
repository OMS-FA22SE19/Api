using Core.Entities;

namespace Core.Interfaces
{
    public interface IReservationRepository : IGenericRepository<Reservation>
    {
        Task<List<Reservation>> GetAllReservationWithDate(DateTime date);
    }
}
