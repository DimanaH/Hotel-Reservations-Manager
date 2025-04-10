using System.ComponentModel.DataAnnotations;

namespace HotelReservationsManager.Models
{
    // Клас за клиенти на хотела
    public class Client
    {
        public int Id { get; set; } // Уникален идентификатор

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } // Собствено име

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } // Фамилно име

        [Required]
        [StringLength(10, ErrorMessage = "Телефонният номер не може да надвишава 10 символа.")]
        public string PhoneNumber { get; set; } // Телефонен номер

        [Required]
        [EmailAddress]
        public string Email { get; set; } // Имейл

        [Required]
        public bool IsAdult { get; set; } // Възрастен (над 18 г.)
    }
}