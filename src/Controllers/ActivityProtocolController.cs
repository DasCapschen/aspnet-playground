using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using src.Data;
using src.Models;

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
            IQueryable<ActivityProtocol> data = _context.ActivityProtocols;
            if(SearchQuery != "") 
            {
                //doing this up-front causes exception... "the query switched to client-evaluation"
                //var tsQuery = EF.Functions.PlainToTsQuery(SearchQuery);
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

            var activityProtocol = await _context.ActivityProtocols
                .FirstOrDefaultAsync(m => m.Id == id);
            if (activityProtocol == null)
            {
                return NotFound();
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
        public async Task<IActionResult> Create([Bind("Id,Date,JournalEntry")] ActivityProtocol activityProtocol)
        {
            if (ModelState.IsValid)
            {
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

            var auth = await _authorizationService.AuthorizeAsync(User, activityProtocol, "OneDayEditPolicy");
            if(!auth.Succeeded) {
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
                    var auth = await _authorizationService.AuthorizeAsync(User, oldProtocol, "OneDayEditPolicy");
                    if(!auth.Succeeded) {
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

            var activityProtocol = await _context.ActivityProtocols
                .FirstOrDefaultAsync(m => m.Id == id);
            if (activityProtocol == null)
            {
                return NotFound();
            }

            return View(activityProtocol);
        }

        // POST: ActivityProtocol/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var activityProtocol = await _context.ActivityProtocols.FindAsync(id);
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
