namespace SmartManager.Models
{
    public record class FaceInfoSimple
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public string? Sex { get; set; }
        public string? Age { get; set; }
        public string? JoinTime { get; set; }

        public FaceInfoSimple(string uid, string name, string? sex, string? age, string? joinTime)
        {
            Uid = uid;
            Name = name;
            Sex = sex;
            Age = age;
            JoinTime = joinTime;
        }
    }
}
