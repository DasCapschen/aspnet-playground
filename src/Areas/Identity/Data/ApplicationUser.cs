using System;
using Microsoft.AspNetCore.Identity;

namespace src.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        //CultureInfo, eg "de-DE"
        public string Culture { get; set; }

        //TimeZoneInfo, eg "Europe/Berlin" :)
        public string TimeZoneId { get; set; }

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