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

        public ActivityProtocolController(ApplicationDbContext context)
        {
            _context = context;
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
                    var change = _context.Update(activityProtocol);
                    //change.property().OriginalValue is already the CHANGED one... what!?
                    //var max_date = change.Property(p => p.Date).OriginalValue.AddHours(24);
                    var max_date = change.GetDatabaseValues().GetValue<DateTimeOffset>("Date").AddHours(24);
                    if(DateTimeOffset.UtcNow > max_date)
                    {
                        //is forbid right here, or should we rather just undo changes?
                        return Forbid();
                    }

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
