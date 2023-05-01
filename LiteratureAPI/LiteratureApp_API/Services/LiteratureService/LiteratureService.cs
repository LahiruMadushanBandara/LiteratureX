using LiteratureApp_API.Models.Literature;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace LiteratureApp_API.Services.LiteratureService
{
    public class LiteratureService : ILiteratureService
    {
        public Lazy<List<Literature>> _literatures = new Lazy<List<Literature>>(LoadLiteratureData);


        public readonly static int _literaturesToRecommend = 6;
        private readonly static int _trendingLiteraturesCount = 20;
        private List<Literature> _trendingLiteratures = LoadTrendingLiteratures();
        public readonly static string _modelpath = @"model.zip";
        private readonly IHostingEnvironment _hostingEnvironment;

        public List<Literature> GetTrendingLiteratures => LoadTrendingLiteratures();

        public LiteratureService(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public static List<Literature> LoadTrendingLiteratures()
        {
            List<Literature> result = new List<Literature>();

            result.Add(new Literature { LiteratureID = 1573, LiteratureName = "Face/Off (1997)" });
            result.Add(new Literature { LiteratureID = 1721, LiteratureName = "Titanic (1997)" });
            result.Add(new Literature { LiteratureID = 1703, LiteratureName = "Home Alone 3 (1997)" });
            result.Add(new Literature { LiteratureID = 49272, LiteratureName = "Casino Royale (2006)" });
            result.Add(new Literature { LiteratureID = 5816, LiteratureName = "Harry Potter and the Chamber of Secrets (2002)" });
            result.Add(new Literature { LiteratureID = 3578, LiteratureName = "Gladiator (2000)" });
            return result;
        }

        public Literature Get(int id)
        {
            return _literatures.Value.Single(m => m.LiteratureID == id);
        }

        public IEnumerable<Literature> GetAllLiteratures()
        {
            return _literatures.Value;
        }


        public IEnumerable<Literature> GetSomeSuggestions()
        {
            Literature[] literatures = GetRecentLiterature().ToArray();

            Random rnd = new Random();
            int[] literatureSelector = new int[_literaturesToRecommend];

            for (int i = 0; i < _literaturesToRecommend; i++)
            {
                literatureSelector[i] = rnd.Next(literatures.Length);
            }

            return literatureSelector.Select(s => literatures[s]);
        }

        public string GetModelPath()
        {
            return Path.Combine(_hostingEnvironment.ContentRootPath, "MLModel", _modelpath);
        }


        public IEnumerable<Literature> GetRecentLiterature()
        {
            return GetAllLiteratures()
                .Where(m => m.LiteratureName.Contains("20")
                            || m.LiteratureName.Contains("198")
                            || m.LiteratureName.Contains("199"));
        }

        private static List<Literature> LoadLiteratureData()
        {
            List<Literature> result = new List<Literature>();

            FileStream fileReader = File.OpenRead("Data/literatures.csv");

            StreamReader reader = new StreamReader(fileReader);
            try
            {
                bool header = true;
                int index = 0;
                string line = "";
                while (!reader.EndOfStream)
                {
                    if (header)
                    {
                        line = reader.ReadLine();
                        header = false;
                    }
                    line = reader.ReadLine();
                    string[] fields = line.Split(',');
                    int LiteratureID = int.Parse(fields[0].TrimStart(new char[] { '0' }));
                    string LiteratureName = fields[1];
                    result.Add(new Literature() { LiteratureID = LiteratureID, LiteratureName = LiteratureName });
                    index++;
                }
            }
            finally
            {
                reader?.Dispose();
            }

            return result;
        }

        public float Sigmoid(float x)
        {
            return (float)(100 / (1 + Math.Exp(-x)));
        }
    }
}
