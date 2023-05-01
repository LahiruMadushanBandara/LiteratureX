using LiteratureApp_API.Models.Literature;

namespace LiteratureApp_API.Services.LiteratureService
{
    public interface ILiteratureService
    {
        Literature Get(int id);
        IEnumerable<Literature> GetAllLiteratures();
        string GetModelPath();
        IEnumerable<Literature> GetRecentLiterature();
        IEnumerable<Literature> GetSomeSuggestions();

        List<Literature> GetTrendingLiteratures { get; }
        float Sigmoid(float x);
    }
}
