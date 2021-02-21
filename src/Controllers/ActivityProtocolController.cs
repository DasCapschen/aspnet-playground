using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using src.Data;
using src.Models;

namespace src.Controllers
{
    public class ActivityProtocolController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityProtocolController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ActivityProtocol
        public async Task<IActionResult> Index()
        {
            return View(await _context.ActivityProtocols.ToListAsync());
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

            //I hope this works as intended.
            //website does not allow edit if time > protocoltime + 24h, 
            //but if we still somehow receive a POST to edit, forbid it!
            var _oldProtocol = _context.ActivityProtocols.Find(id);
            if(DateTimeOffset.UtcNow > _oldProtocol.Date.AddHours(24)) 
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(activityProtocol);
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
