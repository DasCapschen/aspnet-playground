using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace src.Validation
{
    class ValidateCulture : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string cultureInfo = (string)value;
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach(var c in cultures) {
                if(cultureInfo == c.Name) return true;
            }
            return false;
        }
    }
}