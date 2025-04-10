using HotelReservationsManager.Models;

namespace HotelReservationsManager.Services
{
    public interface IClientService
    {
        Task<Client> GetClientByIdAsync(int id);
        Task<List<Client>> GetAllClientsAsync(int page, int pageSize, string filter = null);
        Task<int> GetClientsCountAsync(string filter = null);
        Task CreateClientAsync(Client client);
        Task UpdateClientAsync(Client client);
        Task DeleteClientAsync(int id);
    }
}