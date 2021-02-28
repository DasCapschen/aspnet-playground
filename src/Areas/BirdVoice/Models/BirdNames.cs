using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace src.Areas.BirdVoice.Models
{
    public class BirdNames : IEquatable<BirdNames>
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        public int? GbifId { get; set; } = null;

        [Column("de")]
        public string German { get; set; }

        [Column("latin")]
        public string Latin { get; set; }

        public BirdNames()
        {

        }

        public bool Equals(BirdNames other)
        {
            if (other is null)
                return false;

            return Id == other.Id;
        }

        public override bool Equals(object obj) => Equals(obj as BirdNames);
        public override int GetHashCode() => (Id).GetHashCode();
    }
}