using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using src.Areas.Identity.Data;

namespace src.Areas.BirdVoice.Models
{
    public class UserBirdStats
    {
        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey("Bird")]
        public int BirdId { get; set; }
        public BirdNames Bird { get; set; }

        /// how often the user correctly identified this bird
        public int AnswersCorrect { get; set; } = 0;

        /// how often the user did not identify this bird
        public int AnswersWrong { get; set; } = 0;

        /// how often the user was asked to identify this bird
        public int AnswerCount => AnswersCorrect + AnswersWrong;

        public UserBirdStats()
        {

        }
    }
}