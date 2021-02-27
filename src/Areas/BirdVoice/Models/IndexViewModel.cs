using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using src.Areas.Identity.Data;

namespace src.Areas.BirdVoice.Models
{
    public class IndexViewModel
    {
        public ApplicationUser User { get; set; }
        public List<SelectListItem> AvailableBirdNames { get; set; }
        public List<SelectListItem> ActiveBirdNames { get; set; }

        public IndexViewModel(List<BirdNames> available, List<BirdNames> active)
        {
            AvailableBirdNames = new List<SelectListItem>();
            foreach(var bird in available)
            {
                AvailableBirdNames.Add(new SelectListItem{
                    Value = $"{bird.Id}",
                    Text = $"{bird.German} ({bird.Latin})"
                });
            }

            ActiveBirdNames = new List<SelectListItem>();
            foreach(var bird in active)
            {
                ActiveBirdNames.Add(new SelectListItem{
                    Value = $"{bird.Id}",
                    Text = $"{bird.German} ({bird.Latin})"
                });
            }
        }
    }
}