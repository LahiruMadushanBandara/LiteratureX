namespace LiteratureApp_API.Models.Literature
{
    public class Literature
    {
        public int LiteratureID { get; set; }
        public string LiteratureName { get; set; }

        public bool liked;

        public float RatingScore { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }
    }
}
