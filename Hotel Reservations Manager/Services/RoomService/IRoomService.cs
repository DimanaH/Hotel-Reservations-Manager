using HotelReservationsManager.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelReservationsManager.Services
{
    public interface IRoomService
    {
        Task<List<Room>> GetAllRoomsAsync(int page, int pageSize, int? capacity = null, RoomType? type = null, bool? isAvailable = null);
        Task<int> GetRoomsCountAsync(int? capacity = null, RoomType? type = null, bool? isAvailable = null);
        Task<Room> GetRoomByIdAsync(int id);
        Task CreateRoomAsync(Room room);
        Task UpdateRoomAsync(Room room);
        Task DeleteRoomAsync(int id);
        Task<List<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut);
        Task UpdateRoomAvailabilityAsync();
    }
}