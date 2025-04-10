using System.ComponentModel.DataAnnotations;

namespace HotelReservationsManager.Attributes // Адаптирайте namespace спрямо вашия проект
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;

        public DateGreaterThanAttribute(string otherProperty)
        {
            _otherProperty = otherProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var otherValue = validationContext.ObjectType
                .GetProperty(_otherProperty)
                ?.GetValue(validationContext.ObjectInstance) as DateTime?;

            var currentValue = value as DateTime?;

            if (currentValue.HasValue && otherValue.HasValue && currentValue <= otherValue)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}