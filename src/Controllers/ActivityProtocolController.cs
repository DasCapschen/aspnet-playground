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
using src.Data;
using src.Models;
using src.Policies;

namespace src.Controllers
{
    [Authorize]
    public class ActivityProtocolController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IAuthorizationService _authorizationService;

        public ActivityProtocolController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        // GET: ActivityProtocol
        public async Task<IActionResult> Index(string SearchQuery = "")
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            IQueryable<ActivityProtocol> data = from p in _context.ActivityProtocols
                where p.OwnerId == userId
                select p;

            if(SearchQuery != "" && SearchQuery != StringValues.Empty) 
            {
                //FIXME: this is slow because we do not have an index!
                data = from p in _context.ActivityProtocols
                       where EF.Functions.ToTsVector(p.JournalEntry).Matches(EF.Functions.PlainToTsQuery(SearchQuery))
                       || p.Entries.Any(e => EF.Functions.ToTsVector(e.Description).Matches(EF.Functions.PlainToTsQuery(SearchQuery)))
                       select p;
            }
            return View(await data.ToListAsync());
        }

        // GET: ActivityProtocol/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityProtocol = await _context.ActivityProtocols.FirstOrDefaultAsync(m => m.Id == id);
            if (activityProtocol == null)
            {
                return NotFound();
            }

            var auth = await _authorizationService.AuthorizeAsync(User, activityProtocol, "UserIsOwnerPolicy");
            if(!auth.Succeeded)
            {
                return Forbid();
            }

            return View(activityProtocol);
        }

        // GET: ActivityProtocol/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ActivityProtocol/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,JournalEntry")] ActivityProtocol activityProtocol)
        {
            if (ModelState.IsValid)
            {
                var ownerId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

                activityProtocol.Date = DateTimeOffset.UtcNow;
                activityProtocol.OwnerId = ownerId;

                _context.Add(activityProtocol);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(activityProtocol);
        }

        // GET: ActivityProtocol/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityProtocol = await _context.ActivityProtocols.FindAsync(id);
            if (activityProtocol == null)
            {
                return NotFound();
            }

            var requirements = new IAuthorizationRequirement[] {
                new UserIsOwnerRequirement(), new OneDayEditRequirement()
            };
            var editAuth = await _authorizationService.AuthorizeAsync(User, activityProtocol, "OneDayEditPolicy");
            var ownerAuth = await _authorizationService.AuthorizeAsync(User, activityProtocol, "UserIsOwnerPolicy");
            if(!ownerAuth.Succeeded)
            {
                return Forbid();
            }
            if(!editAuth.Succeeded)
            {
                return View("EditForbidden");
            }

            return View(activityProtocol);
        }

        // POST: ActivityProtocol/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,JournalEntry")] ActivityProtocol activityProtocol)
        {
            if (id != activityProtocol.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //get old protocol values (warning: context starts change-tracking it!)
                    ActivityProtocol oldProtocol = _context.ActivityProtocols.First(p => p.Id == id);

                    //auth action
                    var requirements = new IAuthorizationRequirement[] {
                        new UserIsOwnerRequirement(), new OneDayEditRequirement()
                    };
                    var auth = await _authorizationService.AuthorizeAsync(User, oldProtocol, requirements);
                    if(!auth.Succeeded) 
                    {
                        return Forbid();
                    }

                    //detach old protocol so we can update new protocol below
                    _context.Entry(oldProtocol).State = EntityState.Detached;

                    var change = _context.Update(activityProtocol);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActivityProtocolExists(activityProtocol.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(activityProtocol);
        }

        // GET: ActivityProtocol/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityProtocol = await _context.ActivityProtocols.FirstOrDefaultAsync(m => m.Id == id);
            if (activityProtocol == null)
            {
                return NotFound();
            }

            //auth action
            var requirements = new IAuthorizationRequirement[] { new UserIsOwnerRequirement() };
            var auth = await _authorizationService.AuthorizeAsync(User, activityProtocol, requirements);
            if(!auth.Succeeded) 
            {
                return Forbid();
            }

            return View(activityProtocol);
        }

        // POST: ActivityProtocol/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var activityProtocol = await _context.ActivityProtocols.FindAsync(id);

            //auth action
            var requirements = new IAuthorizationRequirement[] { new UserIsOwnerRequirement() };
            var auth = await _authorizationService.AuthorizeAsync(User, activityProtocol, requirements);
            if(!auth.Succeeded) 
            {
                return Forbid();
            }

            _context.ActivityProtocols.Remove(activityProtocol);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActivityProtocolExists(int id)
        {
            return _context.ActivityProtocols.Any(e => e.Id == id);
        }
    }
}
