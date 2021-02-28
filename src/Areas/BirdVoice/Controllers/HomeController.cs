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
using System.Text.Json;

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
            var bird = await GetRandomActiveBirdAsync();
            if(!bird.GbifId.HasValue) 
            {
                var (gbifName, gbifId) = await GetGBIFNameAsync(bird.Latin);
                await TryUpdateModelAsync(bird, "", b => b.GbifId, b => b.Latin);
                bird.GbifId = gbifId;
                bird.Latin = gbifName;
                await _context.SaveChangesAsync();
            }

            var (audioUrl, audioLicense) = await GetXenoCantoAsync(bird.Latin);
            var (pictureUrl, pictureLicense) = await GetGBIFPictureAsync(bird.GbifId.Value);

            var model = new StudyViewModel {
                Bird = bird,
                AudioUrl = audioUrl,
                AudioLicense = audioLicense,
                PictureUrl = pictureUrl,
                PictureLicense = pictureLicense,
            };

            return View(model);
        }

        public async Task<IActionResult> Quiz()
        {
            return View();
        }

        private async Task<BirdNames> GetRandomActiveBirdAsync()
        {
            var userId = _userManager.GetUserId(User);

            var activeBirds = await _context.Users.Where(u => u.Id == userId)
                .Include(u => u.ActiveBirds).SelectMany(u => u.ActiveBirds)
                .Include(ab => ab.Bird).ToListAsync();

            var random = new Random();
            return activeBirds[random.Next(0, activeBirds.Count)].Bird;
        }

        private async Task<(string, string)> GetXenoCantoAsync(string latin_name)
        {
            var random = new Random();

            using (WebClient wc = new WebClient())
            {
                var jsonStr = await wc.DownloadStringTaskAsync($"https://www.xeno-canto.org/api/2/recordings?query={latin_name}+q_gt:C");
                var json = JsonDocument.Parse(jsonStr);
                var length = json.RootElement.GetProperty("recordings").GetArrayLength();

                if(length == 0) throw new Exception("Bird does not exist!");

                var index = random.Next(0, length);
                var recording = json.RootElement.GetProperty("recordings")[index];
                var audioUrl = recording.GetProperty("file").GetString();
                var audioLicense = recording.GetProperty("lic").GetString();
                var audioRecorder = recording.GetProperty("rec").GetString();

                return (audioUrl, $"https:{audioLicense} (recorded by {audioRecorder})");
            }
        }

        private async Task<(string, int)> GetGBIFNameAsync(string latin_name)
        {
            using (WebClient wc = new WebClient())
            {
                var gbif = await wc.DownloadStringTaskAsync($"https://api.gbif.org/v1/species/match?name={latin_name}");
                var json = JsonDocument.Parse(gbif);
                var name = json.RootElement.GetProperty("species").GetString();
                var id = json.RootElement.GetProperty("speciesKey").GetInt32();
                return (name,id);
            }
        }

        private async Task<(string, string)> GetGBIFPictureAsync(int id) 
        {
            var rand = new Random();

            using (WebClient wc = new WebClient())
            {
                var gbif = await wc.DownloadStringTaskAsync($"https://api.gbif.org/v1/species/{id}/media");
                var json = JsonDocument.Parse(gbif);
                var length = json.RootElement.GetProperty("results").GetArrayLength();
                while(true)
                {
                    var index = rand.Next(0,length);
                    var img = json.RootElement.GetProperty("results")[index];
                    var type = img.GetProperty("type").GetString();
                    //TODO: this might be illegal, check property "license" !!!
                    // we cannot query for free licenses :(
                    if(type == "StillImage") 
                    {
                        var url = img.GetProperty("identifier").GetString();
                        var license = img.GetProperty("license").GetString();
                        var takenBy = img.GetProperty("rightsHolder").GetString();
                        return (url, $"{license} (taken by {takenBy})");
                    }
                }
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