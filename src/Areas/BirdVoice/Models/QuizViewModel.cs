
namespace src.Areas.BirdVoice.Models
{
    public class QuizViewModel
    {
        public BirdNames Bird1 { get; set; }
        public BirdNames Bird2 { get; set; }
        public BirdNames Bird3 { get; set; }
        public BirdNames Bird4 { get; set; }

        public int CorrectBird { get; set; }

        public string AudioUrl { get; set; } = "";
        public string PictureUrl { get; } = "/no-birb.svg";
        public string AudioLicense { get; set; } = "";

        public QuizViewModel() 
        {

        }
    }
}