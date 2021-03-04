using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using src.Areas.Identity.Data;

namespace src.Areas.BirdVoice.Models
{
    public class UserBirdPriority
    {
        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey("Bird")]
        public int BirdId { get; set; }
        public BirdNames Bird { get; set; }

        /// how often the user correctly identified this bird
        private int _priority;
        public int Priority { 
            get => _priority;
            set {
                _priority = Math.Clamp(value, 1, 100);
            }
        }

        public UserBirdPriority()
        {
            Priority = 100;
        }
    }
}