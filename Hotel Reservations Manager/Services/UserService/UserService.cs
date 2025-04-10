using HotelReservationsManager.Data;
using HotelReservationsManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelReservationsManager.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync(int page, int pageSize, string filter = null)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(u => u.UserName.Contains(filter) ||
                                        u.FirstName.Contains(filter) ||
                                        u.MiddleName.Contains(filter) ||
                                        u.LastName.Contains(filter) ||
                                        u.Email.Contains(filter));
            }

            return await query.OrderBy(u => u.UserName)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync();
        }

        public async Task<int> GetUsersCountAsync(string filter = null)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(u => u.UserName.Contains(filter) ||
                                        u.FirstName.Contains(filter) ||
                                        u.MiddleName.Contains(filter) ||
                                        u.LastName.Contains(filter) ||
                                        u.Email.Contains(filter));
            }

            return await query.CountAsync();
        }

        public async Task CreateUserAsync(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to update user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }

        public async Task<bool> IsUserActiveAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user?.IsActive ?? false;
        }
    }
}