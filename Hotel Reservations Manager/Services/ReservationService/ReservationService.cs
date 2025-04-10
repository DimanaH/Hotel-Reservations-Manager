using HotelReservationsManager.Data;
using HotelReservationsManager.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationsManager.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;

        public ReservationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Reservation> GetReservationByIdAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .Include(r => r.Clients)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Reservation>> GetAllReservationsAsync(int page, int pageSize, string filter = null)
        {
            var query = _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(r => r.User.UserName.Contains(filter) || r.Room.RoomNumber.Contains(filter));
            }

            return await query.OrderBy(r => r.CheckInDate)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync();
        }

        public async Task<int> GetReservationsCountAsync(string filter = null)
        {
            var query = _context.Reservations.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(r => r.User.UserName.Contains(filter) || r.Room.RoomNumber.Contains(filter));
            }

            return await query.CountAsync();
        }


        public async Task CreateReservationAsync(Reservation reservation)
        {
            reservation.TotalAmount = await CalculateTotalAmountAsync(reservation);
            var room = await _context.Rooms.FindAsync(reservation.RoomId);
            if (room != null)
            {
                room.IsAvailable = false;
            }
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReservationAsync(Reservation reservation)
        {
            var existingReservation = await _context.Reservations
                .Include(r => r.Clients)
                .FirstOrDefaultAsync(r => r.Id == reservation.Id);

            if (existingReservation == null)
            {
                throw new Exception("Reservation not found.");
            }

            // Актуализираме основните полета
            _context.Entry(existingReservation).CurrentValues.SetValues(reservation);

            // Премахваме старите връзки с клиенти
            var existingClientIds = existingReservation.Clients.Select(c => c.Id).ToList();
            var clientsToRemove = existingReservation.Clients
                .Where(c => !reservation.Clients.Any(nc => nc.Id == c.Id))
                .ToList();
            foreach (var client in clientsToRemove)
            {
                existingReservation.Clients.Remove(client);
            }

            // Добавяме новите клиенти
            var newClientIds = reservation.Clients.Select(c => c.Id).ToList();
            var clientsToAdd = reservation.Clients
                .Where(c => !existingClientIds.Contains(c.Id))
                .ToList();
            foreach (var client in clientsToAdd)
            {
                existingReservation.Clients.Add(client);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                var room = await _context.Rooms.FindAsync(reservation.RoomId);
                if (room != null && reservation.CheckOutDate <= DateTime.Now)
                {
                    room.IsAvailable = true;
                }
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> CalculateTotalAmountAsync(Reservation reservation)
        {
            var room = await _context.Rooms.FindAsync(reservation.RoomId);
            if (room == null) return 0;

            var days = (reservation.CheckOutDate - reservation.CheckInDate).Days;
            var adults = reservation.Clients.Count(c => c.IsAdult);
            var children = reservation.Clients.Count(c => !c.IsAdult);

            decimal total = (adults * room.AdultPricePerBed + children * room.ChildPricePerBed) * days;

            if (reservation.IsAllInclusive)
            {
                total *= 1.5m; // Примерен коефициент за all inclusive
            }
            else if (reservation.IncludesBreakfast)
            {
                total += 10m * (adults + children) * days; // Примерна цена за закуска
            }

            return total;
        }
    }
}