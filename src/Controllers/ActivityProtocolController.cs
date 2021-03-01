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

        //really?
        private IQueryable<ActivityProtocol> QueryUserProtocols()
        {
            var userId = _userManager.GetUserId(User);
            return _context.Users.Where(u => u.Id == userId)
                .Include(u => u.Protocols).SelectMany(u => u.Protocols)
                .Include(p => p.Entries).AsNoTracking();
        }
        private async Task<ActivityProtocol> GetUserProtocolAsync(int id)
        {
            return await QueryUserProtocols().Where(p => p.Id == id).FirstOrDefaultAsync();
        }

        // GET: ActivityProtocol
        public async Task<IActionResult> Index(string SearchQuery = "")
        {
            var userProtocols = QueryUserProtocols();

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

            var protocol = await GetUserProtocolAsync(id.Value);
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
                protocol.Date = DateTime.UtcNow;
                protocol.UserId = _userManager.GetUserId(User);

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

            var protocol = await GetUserProtocolAsync(id.Value);
            if (protocol == null)
            {
                return NotFound();
            }

            var auth = await _authorizationService.AuthorizeAsync(User, protocol, "OneDayEditPolicy");
            if(!auth.Succeeded)
            {
                return View("EditForbidden");
            }

            return View(protocol);
        }

        // POST: ActivityProtocol/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProtocol(int id)
        {
            //won't work because we get it as NoTracking()
            //var protocol = await GetUserProtocolAsync(id);
            var userId = _userManager.GetUserId(User);
            var protocol = await _context.Users.Where(u => u.Id == userId)
                .Include(u => u.Protocols).SelectMany(u => u.Protocols)
                .Include(p => p.Entries)
                .Where(p => p.Id == id).FirstOrDefaultAsync();

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
            //TODO: allowing all of Entries is probably a bad idea! Only allow Entry.Time and Entry.Description. But how?
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
            else 
            {
                Console.WriteLine("Nope, you cannot update this way!");
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // POST: ActivityProtocol/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var protocol = await GetUserProtocolAsync(id);
            if (protocol == null)
            {
                return NotFound();
            }

            _context.Remove(protocol);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
