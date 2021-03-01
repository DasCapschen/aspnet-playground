
namespace src.Areas.BirdVoice.Models
{
    public class StudyViewModel
    {
        public BirdNames Bird { get; set; }
        public string AudioUrl { get; set; } = "";
        public string PictureUrl { get; set; } = "/no-birb.svg";

        public string PictureLicense { get; set; } = "";
        public string AudioLicense { get; set; } = "";

        public StudyViewModel() 
        {

        }
    }
}