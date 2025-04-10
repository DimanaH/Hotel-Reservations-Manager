using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HotelReservationsManager.Models
{
    // Персонализиран клас за потребители, наследяващ IdentityUser
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } // Собствено име

        [Required]
        [StringLength(50)]
        public string MiddleName { get; set; } // Бащино име

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } // Фамилно име

        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "ЕГН трябва да е точно 10 символа.")]
        public string EGN { get; set; } // ЕГН

        [Required]
        [StringLength(10, ErrorMessage = "Телефонният номер не може да надвишава 10 символа.")]
        public string PhoneNumber { get; set; } // Телефонен номер (преdefinира се от IdentityUser)

        [Required]
        public DateTime HireDate { get; set; } // Дата на назначаване

        public bool IsActive { get; set; } = true; // Активен или неактивен акаунт

        public DateTime? ReleaseDate { get; set; } // Дата на освобождаване (опционална)
    }
}