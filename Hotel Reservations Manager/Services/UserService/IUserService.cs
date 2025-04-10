using HotelReservationsManager.Models;

namespace HotelReservationsManager.Services
{
    public interface IUserService
    {
        Task<ApplicationUser> GetUserByIdAsync(string id);
        Task<List<ApplicationUser>> GetAllUsersAsync(int page, int pageSize, string filter = null);
        Task<int> GetUsersCountAsync(string filter = null);
        Task CreateUserAsync(ApplicationUser user, string password);
        Task UpdateUserAsync(ApplicationUser user);
        Task DeleteUserAsync(string id);
        Task<bool> IsUserActiveAsync(string id);
    }
}