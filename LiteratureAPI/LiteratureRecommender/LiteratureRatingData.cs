using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace LiteratureRecommender
{
    public class LiteratureRating
    {
        [LoadColumn(0)]
        public float userId;
        [LoadColumn(1)]
        public float literatureId;
        [LoadColumn(2)]
        public float Label;
    }

    public class LiteratureRatingPrediction
    {
        public float Label;
        public float Score;
    }
}
