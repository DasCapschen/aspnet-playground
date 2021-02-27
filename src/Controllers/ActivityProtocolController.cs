using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using src.Areas.Identity.Data;
using src.Data;
using src.Extensions;
using src.Models;
using src.Policies;

namespace src.Controllers
{
    [Authorize]
    public class ActivityProtocolController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ActivityProtocolController(ApplicationDbContext context, 
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _authorizationService = authorizationService;
            _userManager = userManager;
        }

        private DateTime ConvertTimeToUTC(DateTime time)
        {
            var timezone = TimeZoneInfo.FindSystemTimeZoneById(User.GetTimeZone());
            return TimeZoneInfo.ConvertTimeToUtc(time, timezone);
        }

        // GET: ActivityProtocol
        public async Task<IActionResult> Index(string SearchQuery = "")
        {
            var userId = _userManager.GetUserId(User);

            //really?
            var userProtocols = _context.Users.Where(u => u.Id == userId).SelectMany(u => u.Protocols).Include(p => p.Entries).AsNoTracking();

            if(SearchQuery != "" && SearchQuery != StringValues.Empty) 
            {
                //TODO: this is slow because we do not have an index!
                //omg
                userProtocols = userProtocols.Where(p => 
                    EF.Functions.ToTsVector(p.JournalEntry).Matches(EF.Functions.PlainToTsQuery(SearchQuery))
                    || p.Entries.Any(e => 
                        EF.Functions.ToTsVector(e.Description).Matches(EF.Functions.PlainToTsQuery(SearchQuery))
                    )
                );
            }
            return View(await userProtocols.ToListAsync());
        }

        // GET: ActivityProtocol/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var protocol = user.Protocols.Find(p => p.Id == id);

            if (protocol == null)
            {
                return NotFound();
            }

            return View(protocol);
        }

        // GET: ActivityProtocol/Create
        public IActionResult Create()
        {
            return View();
        }

        // kind of fixed now, returns 405 when trying to access through URL
        [HttpPost]
        public IActionResult AddProtocolEntry(int index)
        {
            ViewData["Index"] = index;
            return PartialView("_NewEntryPartial");
        }

        // POST: ActivityProtocol/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // outdated, I think
        // see here instead https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/crud?view=aspnetcore-5.0
        [HttpPost]
        [ValidateAntiForgeryToken]
        //TODO: allowing all of Entries is probably a bad idea! Only allow Entry.Time and Entry.Description. But how?
        public async Task<IActionResult> Create([Bind("JournalEntry,Entries")] ActivityProtocol protocol)
        {
            //TODO: should try-catch this 
            if (ModelState.IsValid)
            {
                var ownerId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                protocol.Date = DateTime.UtcNow;
                protocol.OwnerId = ownerId;

                //time in HTML form has NO timezone information!! arrives in Users Timezone,
                //we need to save as UTC, so convert...
                for(int i = 0; i < protocol.Entries.Count; i++)
                {
                    protocol.Entries[i].Time = ConvertTimeToUTC(protocol.Entries[i].Time);
                }

                _context.Add(protocol);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = protocol.Id } );
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ActivityProtocol/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var activityProtocol = user.Protocols.Find(p => p.Id == id);
            if (activityProtocol == null)
            {
                return NotFound();
            }

            var auth = await _authorizationService.AuthorizeAsync(User, activityProtocol, "OneDayEditPolicy");
            if(!auth.Succeeded)
            {
                return View("EditForbidden");
            }

            return View(activityProtocol);
        }

        // POST: ActivityProtocol/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProtocol(int id)
        {
            //find protocol (DB hit!!)
            var user = await _userManager.GetUserAsync(User);
            var protocol = user.Protocols.Find(p => p.Id == id);

            //auth action
            var auth = await _authorizationService.AuthorizeAsync(User, protocol, "OneDayEditPolicy");
            if(!auth.Succeeded)
            {
                return Forbid();
            }

            // yay, this actually worked, I am no longer able to POST-inject data \o/
            // ffs, the Id of the post is still in the HTML code ... :|
            // <form action="Edit/18"> for example. I can change the ID there and edit a post I didn't want to...
            // I guess there is nothing I can do about that?
            // same goes for deleting...

            //no need to detach anymore, because we are not tracking changes yet (?)
            //this is what ModelState.IsValid used to do for us, now we do it explicit:
            var updateSuccess = await TryUpdateModelAsync<ActivityProtocol>(protocol, "", 
                p => p.JournalEntry, p => p.Entries);

            // remember to edit entries AFTER TryUpdate!!! we're not tracking changes before!!!

            //time in HTML form has NO timezone information!! arrives in Users Timezone,
            //we need to save as UTC, so convert...
            for(int i = 0; i < protocol.Entries.Count; i++)
            {
                protocol.Entries[i].Time = ConvertTimeToUTC(protocol.Entries[i].Time);
            }

            //TODO: allowing all of p.Entries is probably a bad idea! Only allow Entry.Time and Entry.Description. But how?

            if (updateSuccess)
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                   throw; //TODO: handle error
                }
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: ActivityProtocol/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var protocol = user.Protocols.Find(p => p.Id == id);
            if (protocol == null)
            {
                return NotFound();
            }

            return View(protocol);
        }

        // POST: ActivityProtocol/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var protocol = user.Protocols.Find(p => p.Id == id);

            _context.Remove(protocol);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
