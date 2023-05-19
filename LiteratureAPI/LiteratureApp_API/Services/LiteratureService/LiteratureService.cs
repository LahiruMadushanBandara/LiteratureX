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

            result.Add(new Literature { LiteratureID = 1573, LiteratureName = "The Sea(poetry) - J.Reeves", Description = "The sea is a hungry dog, Giant and grey. He rolls on the beach all day. With his clashing teeth and shaggy jaws Hour upon hour he gnaws The rumbling, tumbling stones, And 'Bones, bones, bones, bones! ' The giant sea-dog moans, Licking his greasy paws.", ImageUrl= "/assets/images/TheSea.jpg" });
            result.Add(new Literature { LiteratureID = 1721, LiteratureName = "A Minor Bird(poetry) - Robert Frost", Description = "The poem is about intolerance of nature, disrespect for others and suppression of the weak by the powerful. The speaker shows a total disregard for the bird which he sees to be 'minor' (insignificant or unimportant)", ImageUrl = "/assets/images/minor-bird.jpg" });
            result.Add(new Literature { LiteratureID = 1703, LiteratureName = "Paying Calls(poetry) - Thomas Hardy", Description = "“Paying Calls”, written by a mature Hardy, is one of his shorter poems. There is a meditative quality throughout the composition.", ImageUrl = "/assets/images/paying-calls.jpg" });
            result.Add(new Literature { LiteratureID = 49272, LiteratureName = "Mother(Novel) - Maxim Gorky", Description = "Mother is a novel written by Maxim Gorky in 1906 about revolutionary factory workers. It was first published, in English, in Appleton's Magazine in 1906, then in Russian in 1907. Although Gorky was highly critical of the novel, the work was translated into many languages", ImageUrl = "/assets/images/Mother.jpg" });
            result.Add(new Literature { LiteratureID = 5816, LiteratureName = "The Village by the Sea(Novel) - Anita Desai", Description = "The Village by the Sea: an Indian family story is a novel for young people by the Indian writer Anita Desai, published in London by Heinemann in 1982. It is based on the poverty, hardships and sorrow faced by a small rural, community in India", ImageUrl = "assets/images/village-by-the-sea.jpg" });
            result.Add(new Literature { LiteratureID = 3578, LiteratureName = "Anne Frank Huis(poetry) - Andrew Motion", Description = "Anne Frank Huis. By Andrew Motion. Even now, after twice her lifetime of grief. and anger in the very place, whoever comes. to climb these narrow stairs", ImageUrl = "/assets/images/frank-huis.jfif" });
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
