using System;
using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace src.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        //CultureInfo, eg "de-DE"
        public string Culture { get; set; } = "en-US";

        //TimeZoneInfo, eg "Europe/Berlin" :)
        public string TimeZoneId { get; set; } = "UTC";

        public ApplicationUser()
            : base()
        {
        }

        public ApplicationUser(string username)
            : base(username)
        {
        }
    }
}