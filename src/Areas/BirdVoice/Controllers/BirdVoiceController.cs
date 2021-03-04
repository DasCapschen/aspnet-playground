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

            var model = new IndexViewModel(await availableBirds.ToListAsync(), await activeBirds.ToListAsync());

            var statsQuery = _context.Users.Where(u => u.Id == userId)
                .Include(u => u.BirdStats).SelectMany(u => u.BirdStats);

            model.TotalAnswers = await statsQuery.SumAsync(bs => bs.AnswersCorrect + bs.AnswersWrong);
            model.TotalCorrect = await statsQuery.SumAsync(bs => bs.AnswersCorrect);
            model.TotalWrong = await statsQuery.SumAsync(bs => bs.AnswersWrong);

            model.BestBird = await statsQuery.Where(bs => (bs.AnswersCorrect + bs.AnswersWrong) > 0)
                .OrderByDescending(bs => bs.AnswersCorrect / (bs.AnswersCorrect + bs.AnswersWrong)).FirstOrDefaultAsync();

            model.WorstBird = await statsQuery.Where(bs => (bs.AnswersCorrect + bs.AnswersWrong) > 0)
                .OrderByDescending(bs => bs.AnswersWrong / (bs.AnswersCorrect + bs.AnswersWrong)).FirstOrDefaultAsync();

            return View(model);
        }

        public async Task<IActionResult> Study()
        {
            BirdNames bird = null;
            try {
                bird = await GetRandomBirdWithPriorityAsync();
            } catch {
                return NotFound();
            }

            var (audioUrl, audioLicense) = await GetXenoCantoAsync(bird);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task StudyAnswer(int birdId, bool known)
        {
            var userId = _userManager.GetUserId(User);

            var birdPriority = await _context.Users.Where(u => u.Id == userId)
                .Include(u => u.BirdPriorities).SelectMany(u => u.BirdPriorities)
                .Where(bp => bp.BirdId == birdId).FirstOrDefaultAsync();
            
            var success = await TryUpdateModelAsync(birdPriority, "", bp => bp.Priority);
            if(success)
            {
                if(known) birdPriority.Priority--;
                else birdPriority.Priority++;
                
                await _context.SaveChangesAsync();
                return;
            }
            throw new Exception("Cannot update bird Priority!");
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
                case 1: (url, license) = await GetXenoCantoAsync(bird1); break;
                case 2: (url, license) = await GetXenoCantoAsync(bird2); break;
                case 3: (url, license) = await GetXenoCantoAsync(bird3); break;
                case 4: (url, license) = await GetXenoCantoAsync(bird4); break;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task QuizAnswer(int birdId, bool correct)
        {
            var userId = _userManager.GetUserId(User);
            var birdStats = _context.Users.Where(u => u.Id == userId)
                .Include(u => u.BirdStats).SelectMany(u => u.BirdStats)
                .Where(bs => bs.BirdId == birdId).FirstOrDefault();

            var success = await TryUpdateModelAsync(birdStats, "", bs => bs.AnswersCorrect, bs => bs.AnswersWrong);
            if(success)
            {
                if(correct) birdStats.AnswersCorrect += 1;
                else birdStats.AnswersWrong += 1;

                await _context.SaveChangesAsync();
                return;
            }
            throw new Exception("Cannot update bird stats!");
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

        private async Task<BirdNames> GetRandomBirdWithPriorityAsync()
        {
            var userId = _userManager.GetUserId(User);

            var activeCount = await _context.Users.Where(u => u.Id == userId)
                .Include(u => u.ActiveBirds).SelectMany(u => u.ActiveBirds).CountAsync();
            var priorities = await _context.Users.Where(u => u.Id == userId)
                .Include(u => u.BirdPriorities).SelectMany(u => u.BirdPriorities)
                .Include(bp => bp.Bird).OrderBy(bp => bp.Priority).ToListAsync();

            var random = new Random();
            double x = random.NextDouble();
            double s = 0.0;
            for(int i = 0; i < priorities.Count; i++)
            {
                s += (priorities[i].Priority/100.0) / activeCount;
                if(x <= s)
                {
                    return priorities[i].Bird;
                }
            }
            //should not happen EVER
            throw new Exception("Failed to generate random bird with priority");
        }

        private async Task<(string, string)> GetXenoCantoAsync(BirdNames bird)
        {
            if(!bird.GbifId.HasValue) 
            {
                var (gbifName, gbifId) = await GetGBIFNameAsync(bird.Latin);
                await TryUpdateModelAsync(bird, "", b => b.GbifId, b => b.Latin);
                bird.GbifId = gbifId;
                bird.Latin = gbifName;
                await _context.SaveChangesAsync();
            }

            var random = new Random();
            using (WebClient wc = new WebClient())
            {
                var jsonStr = await wc.DownloadStringTaskAsync($"https://www.xeno-canto.org/api/2/recordings?query={bird.Latin}+q_gt:C");
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
            var userId = _userManager.GetUserId(User);

            var user = await _context.Users.Where(u => u.Id == userId)
                .Include(u => u.ActiveBirds)
                .Include(u => u.BirdStats)
                .Include(u => u.BirdPriorities)
                .FirstOrDefaultAsync();

            var success = await TryUpdateModelAsync<ApplicationUser>(user, "", 
                user => user.ActiveBirds, user => user.BirdStats, user => user.BirdPriorities);
            if(!success) {
                return Forbid();
            }

            //add the new bird to it
            if(!user.ActiveBirds.Exists(ab => ab.BirdId == id)) {
                user.ActiveBirds.Add(new UserActiveBird {
                    UserId = user.Id,
                    BirdId = id
                });
            }

            //add bird priority entry for active bird
            if(!user.BirdPriorities.Exists(bp => bp.BirdId == id)) {
                user.BirdPriorities.Add(new UserBirdPriority {
                    UserId = user.Id,
                    BirdId = id
                });
            }

            //add user bird stats
            if(!user.BirdStats.Exists(bp => bp.BirdId == id)) {
                user.BirdStats.Add(new UserBirdStats {
                    UserId = user.Id,
                    BirdId = id
                });
            }

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
            //get user and start tracking it
            var userId = _userManager.GetUserId(User);
            var user = await _context.Users.Where(u => u.Id == userId)
                .Include(u => u.ActiveBirds)
                .FirstOrDefaultAsync();

            var success = await TryUpdateModelAsync<ApplicationUser>(user, "", user => user.ActiveBirds);
            if(!success) {
                return Forbid();
            }

            //remove the bird
            var birdToRemove = user.ActiveBirds.Find(p => p.BirdId == id);
            user.ActiveBirds.Remove(birdToRemove);

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