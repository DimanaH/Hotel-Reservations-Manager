using HotelReservationsManager.Data;
using HotelReservationsManager.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationsManager.Services
{
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;

        public ClientService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Client> GetClientByIdAsync(int id)
        {
            return await _context.Clients.FindAsync(id);
        }

        public async Task<List<Client>> GetAllClientsAsync(int page, int pageSize, string filter = null)
        {
            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(c => c.FirstName.Contains(filter) || c.LastName.Contains(filter));
            }

            return await query.OrderBy(c => c.LastName)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync();
        }

        public async Task<int> GetClientsCountAsync(string filter = null)
        {
            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(c => c.FirstName.Contains(filter) || c.LastName.Contains(filter));
            }

            return await query.CountAsync();
        }

        public async Task CreateClientAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateClientAsync(Client client)
        {
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClientAsync(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
        }
    }
}