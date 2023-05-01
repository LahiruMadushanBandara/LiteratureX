namespace LiteratureApp_API.Models.Users
{
    public class UserProfile
    {
        public int ProfileID { get; set; }
        public string ProfileImageName { get; set; }
        public string ProfileName { get; set; }
        public List<(int literatureId, int literatureRating)> ProfileLiteratureRatings { get; set; }
    }
}
