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
using System.Collections.Generic;

namespace src.Areas.BirdVoice.Controllers
{
    [Area("BirdVoice")]
    [Authorize]
    public class BirdVoiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BirdVoiceController(ApplicationDbContext context, 
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
            BirdNames bird = null;
            try {
                bird = await GetRandomActiveBirdAsync();
            } catch {
                return NotFound();
            }

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

            var model = new StudyViewModel{
                Bird = bird,
                AudioUrl = audioUrl,
                AudioLicense = audioLicense
            };

            if(pictureUrl != "") {
                model.PictureUrl = pictureUrl;
                model.PictureLicense = pictureLicense;
            }

            return View(model);
        }

        public async Task<IActionResult> Quiz()
        {
            var random = new Random();
            var correctBird = random.Next(1,5); //[1,5) = [1,4]

            var bird1 = await GetRandomActiveBirdAsync();
            var bird2 = await GetRandomActiveBirdAsync(bird1);
            var bird3 = await GetRandomActiveBirdAsync(bird1, bird2);
            var bird4 = await GetRandomActiveBirdAsync(bird1, bird2, bird3);
            
            //ouch, really?
            string url = "", license = "";
            switch(correctBird) {
                case 1: (url, license) = await GetXenoCantoAsync(bird1.Latin); break;
                case 2: (url, license) = await GetXenoCantoAsync(bird2.Latin); break;
                case 3: (url, license) = await GetXenoCantoAsync(bird3.Latin); break;
                case 4: (url, license) = await GetXenoCantoAsync(bird4.Latin); break;
            }

            var model = new QuizViewModel {
                Bird1 = bird1,
                Bird2 = bird2,
                Bird3 = bird3,
                Bird4 = bird4,
                CorrectBird = correctBird,
                AudioUrl = url,
                AudioLicense = license
            };
            return View(model);
        }

        private async Task<BirdNames> GetRandomActiveBirdAsync(params BirdNames[] except)
        {
            var userId = _userManager.GetUserId(User);

            IQueryable<UserActiveBird> queryBirds = _context.Users.Where(u => u.Id == userId)
                .Include(u => u.ActiveBirds).SelectMany(u => u.ActiveBirds)
                .Include(ab => ab.Bird);

            if(!(except is null)) 
            {
                queryBirds = queryBirds.Where(uab => !except.Contains(uab.Bird));
            }

            var activeBirds = await queryBirds.ToListAsync();

            if(activeBirds.Count() == 0) {
                throw new Exception("No Active Birds!");
            }

            var random = new Random();
            return activeBirds[random.Next(0, activeBirds.Count())].Bird;
        }

        private async Task<(string, string)> GetXenoCantoAsync(string latin_name)
        {
            var random = new Random();

            using (WebClient wc = new WebClient())
            {
                var jsonStr = await wc.DownloadStringTaskAsync($"https://www.xeno-canto.org/api/2/recordings?query={latin_name}+q_gt:C");
                var json = JsonDocument.Parse(jsonStr);
                var length = json.RootElement.GetProperty("recordings").GetArrayLength();

                if(length == 0) throw new Exception("No recording found on Xeno-Canto!");

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
                var offset = 0;
                string gbif = "";
                while(!gbif.Contains("\"endOfRecords\":true"))
                {
                    gbif = await wc.DownloadStringTaskAsync($"https://api.gbif.org/v1/species/{id}/media?offset={offset}");
                    if(!gbif.Contains("\"license\":\"CC")) 
                    {
                        offset += 20;
                        continue;
                    }

                    var json = JsonDocument.Parse(gbif);
                    var length = json.RootElement.GetProperty("results").GetArrayLength();
                    while(true)
                    {
                        var index = rand.Next(0,length);
                        var img = json.RootElement.GetProperty("results")[index];
                        var type = img.GetProperty("type").GetString();
                        var license = img.GetProperty("license").GetString();
                        //TODO: this might be illegal, check property "license" !!!
                        // we cannot query for free licenses :(
                        if(type == "StillImage" && license.Contains("CC")) 
                        {
                            var url = img.GetProperty("identifier").GetString();
                            var takenBy = img.GetProperty("rightsHolder").GetString();
                            return (url, $"{license} (taken by {takenBy})");
                        }
                    }
                }
            }

            return ("", "");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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