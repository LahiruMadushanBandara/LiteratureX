namespace LiteratureApp_API.Entities
{
    public class ProfileLiteratureRating
    {
        public int ProfileLiteratureRatingId { get; set; }
        public int ProfileId { get; set; }
        public string ProfileName { get; set; }
        public string ProfileImageName { get; set; }
        public int LiteratureId { get; set; }
    }
}
