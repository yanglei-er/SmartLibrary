namespace Shared.Models
{
    public record class User
    {
        public string Name { get; set; }
        public string? Sex { get; set; }
        public string? Age { get; set; }
        public string? JoinTime { get; set; }
        public string Feature { get; set; }

        public User(string name, string? sex, string? age, string? joinTime, string feature)
        {
            Name = name;
            Sex = sex;
            Age = age;
            JoinTime = joinTime;
            Feature = feature;
        }
    }
}
