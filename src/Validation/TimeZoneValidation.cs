using System;
using System.ComponentModel.DataAnnotations;

namespace src.Validation
{
    class ValidateTimeZone : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string timeZoneId = (string)value;
            try {
                TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch {
                return false;
            }
            return true;
        }
    }
}