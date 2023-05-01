using Microsoft.ML.Data;

namespace LiteratureApp_API.Data_Structures
{
    public class LiteratureRating
    {
        [LoadColumn(0)]
        public string userId;

        [LoadColumn(1)]
        public string literatureId;

        [LoadColumn(2)]
        public bool Label;
    }
}
