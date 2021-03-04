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

        public UserBirdStats BestBird { get; set; }
        public UserBirdStats WorstBird { get; set; }

        public int TotalAnswers { get; set; }
        public int TotalCorrect { get; set; }
        public int TotalWrong { get; set; }

        public double TotalCorrectPercent => TotalAnswers > 0 ? (double)TotalCorrect / (double)TotalAnswers : 0;

        public IndexViewModel(List<BirdNames> available, List<BirdNames> active)
        {
            AvailableBirdNames = new List<SelectListItem>();
            foreach(var bird in available)
            {
                AvailableBirdNames.Add(new SelectListItem{
                    Value = $"{bird.Id}",
                    Text = bird.FullName
                });
            }

            ActiveBirdNames = new List<SelectListItem>();
            foreach(var bird in active)
            {
                ActiveBirdNames.Add(new SelectListItem{
                    Value = $"{bird.Id}",
                    Text = bird.FullName
                });
            }
        }
    }
}