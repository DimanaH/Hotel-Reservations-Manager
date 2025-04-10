using System.ComponentModel.DataAnnotations;
using HotelReservationsManager.Attributes;

namespace HotelReservationsManager.Models
{
    // Клас за резервации
    public class Reservation
    {
        public int Id { get; set; } // Уникален идентификатор

        [Required]
        public int RoomId { get; set; } // Връзка към стая
        public Room Room { get; set; } // Навигационно свойство

        [Required(ErrorMessage = "The UserId field is required.", AllowEmptyStrings = true)]
        public string UserId { get; set; } // Връзка към потребител
        public ApplicationUser User { get; set; } // Навигационно свойство

        public List<Client> Clients { get; set; } = new List<Client>(); // Списък с клиенти

        [Required]
        public DateTime CheckInDate { get; set; } // Дата на настаняване

        [Required]
        [DateGreaterThan("CheckInDate", ErrorMessage = "Датата на освобождаване трябва да е след датата на настаняване.")]
        public DateTime CheckOutDate { get; set; } // Дата на освобождаване

        public bool IncludesBreakfast { get; set; } // Включена закуска

        public bool IsAllInclusive { get; set; } // All inclusive

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; } // Дължима сума
    }
}