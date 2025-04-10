using HotelReservationsManager.Models;
using HotelReservationsManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HotelReservationsManager.Controllers
{
    [Authorize] // Достъп за всички аутентикирани потребители
    public class ClientsController : Controller
    {
        private readonly IClientService _clientService;
        private readonly IReservationService _reservationService;

        public ClientsController(IClientService clientService, IReservationService reservationService)
        {
            _clientService = clientService;
            _reservationService = reservationService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string filter = null)
        {
            var clients = await _clientService.GetAllClientsAsync(page, pageSize, filter);
            var totalClients = await _clientService.GetClientsCountAsync(filter);
            ViewBag.TotalPages = (int)Math.Ceiling(totalClients / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.PageSizeOptions = new[] { 10, 25, 50 };
            ViewBag.Filter = filter;
            return View(clients);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                await _clientService.CreateClientAsync(client);
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Client client)
        {
            if (ModelState.IsValid)
            {
                await _clientService.UpdateClientAsync(client);
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _clientService.DeleteClientAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Reservations(int id, int page = 1, int pageSize = 10)
        {
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null) return NotFound();

            var allReservations = await _reservationService.GetAllReservationsAsync(1, int.MaxValue);
            var clientReservations = allReservations.Where(r => r.Clients.Any(c => c.Id == id)).ToList();

            var totalReservations = clientReservations.Count;
            var pagedReservations = clientReservations
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalPages = (int)Math.Ceiling(totalReservations / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.PageSizeOptions = new[] { 10, 25, 50 };
            ViewBag.Client = client;

            return View(pagedReservations);
        }
    }
}