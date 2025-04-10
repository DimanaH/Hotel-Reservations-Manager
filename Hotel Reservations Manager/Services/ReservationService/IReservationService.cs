using HotelReservationsManager.Models;

namespace HotelReservationsManager.Services
{
    public interface IReservationService
    {
        Task<Reservation> GetReservationByIdAsync(int id);
        Task<List<Reservation>> GetAllReservationsAsync(int page, int pageSize, string filter = null);
        Task<int> GetReservationsCountAsync(string filter = null);
        Task CreateReservationAsync(Reservation reservation);
        Task UpdateReservationAsync(Reservation reservation);
        Task DeleteReservationAsync(int id);
        Task<decimal> CalculateTotalAmountAsync(Reservation reservation);

    }
}