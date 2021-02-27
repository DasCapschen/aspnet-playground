using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using src.Models;

namespace src.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        //CultureInfo, eg "de-DE"
        public string Culture { get; set; } = "en-US";

        //TimeZoneInfo, eg "Europe/Berlin" :)
        public string TimeZoneId { get; set; } = "UTC";

        //list of protocols this User owns
        public List<ActivityProtocol> Protocols { get; set; }

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