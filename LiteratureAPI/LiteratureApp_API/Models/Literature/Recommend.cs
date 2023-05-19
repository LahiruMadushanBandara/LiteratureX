namespace LiteratureApp_API.Models.Literature
{
    public class Recommend
    {
        public List<Literature> ViewedLiteratures { get; set; }
        public List<Literature> Ratings { get; set; }

        public List<Literature> TrendingLiteratures { get; set; }
    }
}
