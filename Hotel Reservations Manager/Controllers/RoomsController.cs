using HotelReservationsManager.Models;
using HotelReservationsManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;

namespace HotelReservationsManager.Controllers
{
    [Authorize]
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, int? capacity = null, RoomType? type = null, bool? isAvailable = null)
        {
            await _roomService.UpdateRoomAvailabilityAsync();
            var rooms = await _roomService.GetAllRoomsAsync(page, pageSize, capacity, type, isAvailable);
            var totalRooms = await _roomService.GetRoomsCountAsync(capacity, type, isAvailable);
            ViewBag.TotalPages = (int)Math.Ceiling(totalRooms / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.PageSizeOptions = new[] { 10, 25, 50 };
            ViewBag.Capacity = capacity;
            ViewBag.Type = type;
            ViewBag.IsAvailable = isAvailable;
            ViewBag.RoomTypes = new SelectList(Enum.GetValues(typeof(RoomType)));
            return View(rooms);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            ViewBag.RoomTypes = new SelectList(Enum.GetValues(typeof(RoomType)));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(Room room)
        {
            if (ModelState.IsValid)
            {
                await _roomService.CreateRoomAsync(room);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.RoomTypes = new SelectList(Enum.GetValues(typeof(RoomType)));
            return View(room);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null) return NotFound();
            ViewBag.RoomTypes = new SelectList(Enum.GetValues(typeof(RoomType)));
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(Room room)
        {
            if (ModelState.IsValid)
            {
                await _roomService.UpdateRoomAsync(room);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.RoomTypes = new SelectList(Enum.GetValues(typeof(RoomType)));
            return View(room);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Details(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null) return NotFound();
            return View(room);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null) return NotFound();
            return View(room);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _roomService.DeleteRoomAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}