using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace src.Areas.BirdVoice.Models
{
    public class BirdNames
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("de")]
        public string German { get; set; }

        [Column("latin")]
        public string Latin { get; set; }

        public BirdNames()
        {

        }
    }
}