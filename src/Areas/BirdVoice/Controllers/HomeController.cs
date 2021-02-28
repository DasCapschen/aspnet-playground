using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using src.Areas.BirdVoice.Models;
using src.Areas.Identity.Data;
using src.Data;

namespace src.Areas.BirdVoice.Controllers
{
    [Area("BirdVoice")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, 
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _authorizationService = authorizationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var activeBirds = _context.Users
                .Where(u => u.Id == userId).Include(u => u.ActiveBirds)
                .SelectMany(u => u.ActiveBirds).Select(ab => ab.Bird).OrderBy(bn => bn.German);

            var availableBirds = _context.BirdNames.Except(activeBirds).OrderBy(bn => bn.German);

            return View(new IndexViewModel(await availableBirds.ToListAsync(), await activeBirds.ToListAsync()));
        }

        public async Task<IActionResult> Study()
        {
            return View();
        }

        public async Task<IActionResult> Quiz()
        {
            return View();
        }

        [HttpPost]
        public async Task<BirdNames> GetRandomActiveBird()
        {
            var userId = _userManager.GetUserId(User);

            var activeBirds = await _context.Users.Where(u => u.Id == userId)
                .Include(u => u.ActiveBirds).SelectMany(u => u.ActiveBirds)
                .Include(ab => ab.Bird).ToListAsync();

            var random = new Random();
            return activeBirds[random.Next(0, activeBirds.Count)].Bird;
        }

        [HttpPost]
        public string GetXenoCanto(string latin_name)
        {
            using (WebClient wc = new WebClient())
            {
                return wc.DownloadString($"https://www.xeno-canto.org/api/2/recordings?query={latin_name}+q:A");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddActiveBird(int id)
        {
            //get user and start tracking it
            var user = await _userManager.GetUserAsync(User);
            var success = await TryUpdateModelAsync<ApplicationUser>(user, "", user => user.ActiveBirds);
            if(!success)
                return Forbid();

            //get current list of active birds from user
            var activeBirds = await _context.Users.Where(u => u.Id == user.Id)
                .Include(u => u.ActiveBirds).Select(u => u.ActiveBirds).FirstOrDefaultAsync();

            //add the new bird to it
            var birdToAdd = _context.BirdNames.Find(id);
            activeBirds.Add(new UserActiveBird{
                UserId = user.Id,
                BirdId = id,
            });

            //update the list (this is the actual update)
            user.ActiveBirds = activeBirds;

            //write to DB
            try {
                await _context.SaveChangesAsync();
            }
            catch {
                return Forbid();
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveActiveBird(int id)
        {
            //get user and start tracking changes
            var user = await _userManager.GetUserAsync(User);
            var success = await TryUpdateModelAsync<ApplicationUser>(user, "", user => user.ActiveBirds);
            if(!success) 
                return Forbid();

            //get current list
            var activeBirds = await _context.Users.Where(u => u.Id == user.Id)
                .Include(u => u.ActiveBirds).Select(u => u.ActiveBirds).FirstOrDefaultAsync();

            //remove the bird
            var birdToRemove = activeBirds.Find(p => p.BirdId == id);
            activeBirds.Remove(birdToRemove);
            
            //update the list (this is the magic)
            user.ActiveBirds = activeBirds;

            //DB write
            try {
                await _context.SaveChangesAsync();
            }
            catch {
                return Forbid();
            }
            return Ok();
        }
    }
}