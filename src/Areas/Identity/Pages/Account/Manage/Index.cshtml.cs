using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Primitives;
using src.Areas.Identity.Data;
using src.Validation;

namespace src.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [ValidateCulture]
            [Display(Name = "Language")]
            public string Culture { get; set; } = "en-US";

            [ValidateTimeZone]
            [Display(Name = "Time Zone")]
            public string TimeZone { get; set; } = "UTC";

            public List<SelectListItem> AvailableCultures { get; }
            public List<SelectListItem> AvailableTimeZones { get; }

            public InputModel()
            {
                AvailableCultures = new List<SelectListItem>();
                var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                foreach(var culture in cultures)
                {
                    AvailableCultures.Add(new SelectListItem{
                        Value = culture.Name,
                        Text = culture.DisplayName
                    });
                }

                AvailableTimeZones = new List<SelectListItem>();
                var timezones = TimeZoneInfo.GetSystemTimeZones();
                foreach(var timezone in timezones)
                {
                    AvailableTimeZones.Add(new SelectListItem{
                        Value = timezone.Id,
                        Text = timezone.Id // displayName isn't really that good tbh
                    });
                }
            }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var culture = "en-US";
            var timezone = "UTC";

            if(user.Culture != StringValues.Empty && user.Culture != "")
            {
                culture = user.Culture;
            }
            if(user.TimeZoneId != StringValues.Empty && user.TimeZoneId != "")
            {
                timezone = user.TimeZoneId;
            }

            Username = userName;
            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                Culture = culture,
                TimeZone = timezone,
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            if(Input.Culture != user.Culture)
            {
                user.Culture = Input.Culture;
            }
            if(Input.TimeZone != user.TimeZoneId)
            {
                user.TimeZoneId = Input.TimeZone;
            }

            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
