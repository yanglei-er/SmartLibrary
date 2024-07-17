using System.Windows.Media.Imaging;

namespace Shared.Models
{
    public record class User
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public string? Sex { get; set; }
        public string? Age { get; set; }
        public string? JoinTime { get; set; }
        public string Feature { get; set; }
        public BitmapImage FaceImage { get; set; }

        public User(string uid, string name, string? sex, string? age, string? joinTime, string feature, BitmapImage faceImage)
        {
            Uid = uid;
            Name = name;
            Sex = sex;
            Age = age;
            JoinTime = joinTime;
            Feature = feature;
            FaceImage = faceImage;
        }
    }
}
