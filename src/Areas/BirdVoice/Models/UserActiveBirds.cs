using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using src.Areas.Identity.Data;

namespace src.Areas.BirdVoice.Models
{
    public class UserActiveBird : IEquatable<BirdNames>
    {
        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey("Bird")]
        public int BirdId { get; set; }
        public BirdNames Bird { get; set; }

        public UserActiveBird()
        {
            
        }

        public bool Equals(BirdNames other)
        {
            if (other is null)
                return false;

            return BirdId == other.Id;
        }
    }
}