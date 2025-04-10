using HotelReservationsManager.Data;
using HotelReservationsManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservationsManager.Services
{
    public class RoomService : IRoomService
    {
        private readonly ApplicationDbContext _context;

        public RoomService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Room>> GetAllRoomsAsync(int page, int pageSize, int? capacity = null, RoomType? type = null, bool? isAvailable = null)
        {
            var query = _context.Rooms.AsQueryable();

            if (capacity.HasValue)
            {
                query = query.Where(r => r.Capacity == capacity.Value);
            }
            if (type.HasValue)
            {
                query = query.Where(r => r.Type == type.Value);
            }
            if (isAvailable.HasValue)
            {
                query = query.Where(r => r.IsAvailable == isAvailable.Value);
            }

            return await query
                .OrderBy(r => r.RoomNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetRoomsCountAsync(int? capacity = null, RoomType? type = null, bool? isAvailable = null)
        {
            var query = _context.Rooms.AsQueryable();

            if (capacity.HasValue)
            {
                query = query.Where(r => r.Capacity == capacity.Value);
            }
            if (type.HasValue)
            {
                query = query.Where(r => r.Type == type.Value);
            }
            if (isAvailable.HasValue)
            {
                query = query.Where(r => r.IsAvailable == isAvailable.Value);
            }

            return await query.CountAsync();
        }

        public async Task<Room> GetRoomByIdAsync(int id)
        {
            return await _context.Rooms.FindAsync(id);
        }

        public async Task CreateRoomAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRoomAsync(Room room)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRoomAsync(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut)
        {
            var allRooms = await _context.Rooms.ToListAsync();
            var conflictingReservations = await _context.Reservations
                .Where(r => r.CheckInDate < checkOut && r.CheckOutDate > checkIn)
                .Select(r => r.RoomId)
                .ToListAsync();

            return allRooms
                .Where(r => !conflictingReservations.Contains(r.Id) && r.IsAvailable)
                .OrderBy(r => r.RoomNumber)
                .ToList();
        }

        public async Task UpdateRoomAvailabilityAsync()
        {
            var expiredReservations = await _context.Reservations
                .Where(r => r.CheckOutDate < DateTime.Now)
                .Include(r => r.Room)
                .ToListAsync();

            foreach (var reservation in expiredReservations)
            {
                if (!reservation.Room.IsAvailable)
                {
                    reservation.Room.IsAvailable = true;
                    _context.Rooms.Update(reservation.Room);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}