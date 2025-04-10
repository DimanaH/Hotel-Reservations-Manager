using System.ComponentModel.DataAnnotations;

namespace HotelReservationsManager.Models
{
    // Типове стаи като enum
    public enum RoomType
    {
        TwinBeds,      // Две единични легла
        DoubleBed,     // Двойно легло
        Apartment,     // Апартамент
        Penthouse,     // Пентхаус
        Maisonette     // Мезонет
    }

    // Клас за стаи в хотела
    public class Room
    {
        public int Id { get; set; } // Уникален идентификатор

        [Required]
        [Range(1, 10, ErrorMessage = "Капацитетът трябва да е между 1 и 10 души.")]
        public int Capacity { get; set; } // Капацитет

        [Required]
        public RoomType Type { get; set; } // Тип на стаята

        [Required]
        public bool IsAvailable { get; set; } = true; // Свободна или заета

        [Required]
        [Range(0, double.MaxValue)]
        public decimal AdultPricePerBed { get; set; } // Цена за възрастен на легло

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ChildPricePerBed { get; set; } // Цена за дете на легло

        [Required]
        [StringLength(10)]
        public string RoomNumber { get; set; } // Номер на стаята

        public string DisplayText => $"{RoomNumber} ({Type}, Capacity: {Capacity})";
    }
}