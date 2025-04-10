using HotelReservationsManager.Models;
using HotelReservationsManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HotelReservationsManager.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IRoomService _roomService;
        private readonly IClientService _clientService;

        public ReservationsController(IReservationService reservationService, IRoomService roomService, IClientService clientService)
        {
            _reservationService = reservationService;
            _roomService = roomService;
            _clientService = clientService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            await _roomService.UpdateRoomAvailabilityAsync();
            var reservations = await _reservationService.GetAllReservationsAsync(page, pageSize);
            var totalReservations = await _reservationService.GetReservationsCountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalReservations / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.PageSizeOptions = new[] { 10, 25, 50 };
            return View(reservations);
        }

        public async Task<IActionResult> Create()
        {
            var availableRooms = await _roomService.GetAvailableRoomsAsync(DateTime.Now, DateTime.Now.AddDays(1));
            if (!availableRooms.Any())
            {
                ModelState.AddModelError("", "No available rooms exist. Please add a room first.");
            }
            ViewBag.Rooms = new SelectList(availableRooms, "Id", "DisplayText");
            ViewBag.Clients = await _clientService.GetAllClientsAsync(1, int.MaxValue);

            ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
            ViewBag.UserName = User.Identity?.Name;
            ViewBag.UserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return View(new Reservation { CheckInDate = DateTime.Now, CheckOutDate = DateTime.Now.AddDays(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reservation reservation, int[] selectedClients)
        {
            if (!selectedClients.Any())
            {
                ModelState.AddModelError("", "Please select at least one client.");
            }

            reservation.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? "TestUser";
            ModelState.Remove("UserId");
            ModelState.Remove("Room");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                reservation.Clients = (await _clientService.GetAllClientsAsync(1, int.MaxValue))
                    .Where(c => selectedClients.Contains(c.Id)).ToList();

                var room = await _roomService.GetRoomByIdAsync(reservation.RoomId);
                int adults = reservation.Clients.Count(c => c.IsAdult);
                int children = reservation.Clients.Count(c => !c.IsAdult);
                int days = (reservation.CheckOutDate - reservation.CheckInDate).Days;
                decimal baseAmount = (adults * room.AdultPricePerBed + children * room.ChildPricePerBed) * days;
                reservation.TotalAmount = baseAmount;
                if (reservation.IsAllInclusive)
                    reservation.TotalAmount *= 1.5m;
                else if (reservation.IncludesBreakfast)
                    reservation.TotalAmount *= 1.2m;

                await _reservationService.CreateReservationAsync(reservation);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ModelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            ViewBag.Rooms = new SelectList(await _roomService.GetAvailableRoomsAsync(reservation.CheckInDate, reservation.CheckOutDate), "Id", "DisplayText");
            ViewBag.Clients = await _clientService.GetAllClientsAsync(1, int.MaxValue);
            return View(reservation);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();

            // Зареждаме свободните стаи
            var availableRooms = await _roomService.GetAvailableRoomsAsync(reservation.CheckInDate, reservation.CheckOutDate);

            // Добавяме текущата стая, ако не е в списъка със свободни стаи
            var currentRoom = await _roomService.GetRoomByIdAsync(reservation.RoomId);
            if (currentRoom != null && !availableRooms.Any(r => r.Id == currentRoom.Id))
            {
                availableRooms.Insert(0, currentRoom); // Добавяме текущата стая в началото
            }

            ViewBag.Rooms = new SelectList(availableRooms, "Id", "DisplayText", reservation.RoomId);
            ViewBag.Clients = await _clientService.GetAllClientsAsync(1, int.MaxValue);
            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Reservation reservation, int[] selectedClients)
        {
            if (!selectedClients.Any())
            {
                ModelState.AddModelError("", "Please select at least one client.");
            }

            reservation.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? "TestUser";
            ModelState.Remove("UserId");
            ModelState.Remove("Room");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                // Зареждаме съществуващата резервация с клиентите й
                var existingReservation = await _reservationService.GetReservationByIdAsync(reservation.Id);
                if (existingReservation == null)
                {
                    return NotFound();
                }

                // Премахваме текущите клиенти
                existingReservation.Clients.Clear();

                // Добавяме новите избрани клиенти
                var allClients = await _clientService.GetAllClientsAsync(1, int.MaxValue);
                existingReservation.Clients = allClients
                    .Where(c => selectedClients.Contains(c.Id))
                    .ToList();

                // Актуализираме останалите полета
                existingReservation.RoomId = reservation.RoomId;
                existingReservation.CheckInDate = reservation.CheckInDate;
                existingReservation.CheckOutDate = reservation.CheckOutDate;
                existingReservation.IncludesBreakfast = reservation.IncludesBreakfast;
                existingReservation.IsAllInclusive = reservation.IsAllInclusive;

                var room = await _roomService.GetRoomByIdAsync(reservation.RoomId);
                int adults = existingReservation.Clients.Count(c => c.IsAdult);
                int children = existingReservation.Clients.Count(c => !c.IsAdult);
                int days = (existingReservation.CheckOutDate - existingReservation.CheckInDate).Days;
                decimal baseAmount = (adults * room.AdultPricePerBed + children * room.ChildPricePerBed) * days;
                existingReservation.TotalAmount = baseAmount;
                if (existingReservation.IsAllInclusive)
                    existingReservation.TotalAmount *= 1.5m;
                else if (existingReservation.IncludesBreakfast)
                    existingReservation.TotalAmount *= 1.2m;

                await _reservationService.UpdateReservationAsync(existingReservation);
                return RedirectToAction(nameof(Index));
            }

            // При грешка зареждаме отново стаите
            var availableRooms = await _roomService.GetAvailableRoomsAsync(reservation.CheckInDate, reservation.CheckOutDate);
            var currentRoom = await _roomService.GetRoomByIdAsync(reservation.RoomId);
            if (currentRoom != null && !availableRooms.Any(r => r.Id == currentRoom.Id))
            {
                availableRooms.Insert(0, currentRoom);
            }

            ViewBag.Rooms = new SelectList(availableRooms, "Id", "DisplayText", reservation.RoomId);
            ViewBag.Clients = await _clientService.GetAllClientsAsync(1, int.MaxValue);
            ViewBag.ModelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return View(reservation);
        }

        public async Task<IActionResult> Details(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();
            return View(reservation);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null) return NotFound();
            return View(reservation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _reservationService.DeleteReservationAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CalculateTotalAmount([FromBody] ReservationCalculationModel model)
        {
            var room = await _roomService.GetRoomByIdAsync(model.RoomId);
            var clients = (await _clientService.GetAllClientsAsync(1, int.MaxValue))
                .Where(c => model.SelectedClients.Contains(c.Id)).ToList();
            int adults = clients.Count(c => c.IsAdult);
            int children = clients.Count(c => !c.IsAdult);
            int days = (model.CheckOutDate - model.CheckInDate).Days;
            decimal baseAmount = (adults * room.AdultPricePerBed + children * room.ChildPricePerBed) * days;
            decimal totalAmount = baseAmount;
            if (model.IsAllInclusive)
                totalAmount *= 1.5m;
            else if (model.IncludesBreakfast)
                totalAmount *= 1.2m;

            return Json(new { totalAmount });
        }
    }

    public static class RoomExtensions
    {
        public static string DisplayText(this Room room)
        {
            return $"{room.RoomNumber} ({room.Type}, Capacity: {room.Capacity})";
        }
    }

    public class ReservationCalculationModel
    {
        public int RoomId { get; set; }
        public int[] SelectedClients { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public bool IncludesBreakfast { get; set; }
        public bool IsAllInclusive { get; set; }
    }
}