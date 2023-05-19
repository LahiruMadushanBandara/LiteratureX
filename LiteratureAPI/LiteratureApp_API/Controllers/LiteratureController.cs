using LiteratureApp_API.Data_Structures;
using LiteratureApp_API.Models.Literature;
using LiteratureApp_API.Services.LiteratureService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using LiteratureApp_API.Helpers;
using LiteratureApp_API.Services.UserService;

namespace LiteratureApp_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LiteratureController : ControllerBase
    {
        private readonly IUserService _profileService;
        private readonly PredictionEnginePool<LiteratureRating, LiteratureRatingPrediction> _predictionEnginePool;
        private readonly ILiteratureService _literatureService;

        public LiteratureController(
            ILiteratureService literatureService,
            PredictionEnginePool<LiteratureRating, LiteratureRatingPrediction> predictionEnginePool,
            IUserService profileService)
        {
            _profileService = profileService;
            _predictionEnginePool = predictionEnginePool;
            _literatureService = literatureService;
        }


        [HttpGet("{id}")]
        public IActionResult Recommend(int id)
        {
            var activeprofile = _profileService.GetById(id);

            Recommend recommend = new Recommend();

            //1.Create the ML.NET environment and load the already trained model
            MLContext mlContext = new MLContext();

            List<Literature> ratings = new List<Literature>();
            var LiteratureRatings = _profileService.GetProfileViewedLiteratures(id);
            List<Literature> viewedLiteratures = new List<Literature>();

            foreach ((int literatureId, int literatureRating) in LiteratureRatings)
            {
                viewedLiteratures.Add(_literatureService.Get(literatureId));
            }

            LiteratureRatingPrediction prediction = new LiteratureRatingPrediction();

            foreach (var literature in _literatureService.GetTrendingLiteratures)
            {
                //Call the Rating Prediction for each literature prediction
                var input = new LiteratureRating
                {
                    userId = id.ToString(),
                    literatureId = literature.LiteratureID.ToString()
                };

                var predictionHandler = _predictionEnginePool.Predict(modelName: "LiteratureRecommenderModel", input);

                //Normalize the prediction scores for the "ratings" b / w 0 - 100
                float normalizedscore = _literatureService.Sigmoid(predictionHandler.Score);

                //Add the score for recommendation of each literature in the trending literature list
                ratings.Add(new Literature
                  {
                    LiteratureID = literature.LiteratureID,
                    RatingScore = normalizedscore,
                    LiteratureName = literature.LiteratureName,
                    ImageUrl = literature.ImageUrl,
                    Description = literature.Description,
                });
            }

            //3.Provide rating predictions to the view to be displayed
            recommend.ViewedLiteratures = viewedLiteratures;
            recommend.Ratings = ratings;
            recommend.TrendingLiteratures = _literatureService.GetTrendingLiteratures;

            return Ok(recommend);
        }

        [HttpGet("GetTest")]
        public IActionResult GetTest()
        {
            return Ok();
        }

        [HttpGet("GetUser")]
        public IActionResult GetById(int id)
        {
            var user = _profileService.GetById(id);
            return Ok(user);
        }
    }

    class Rating
    {
        public int LiteratureID { get; set; }
        public float normalizedScore { get; set; }
    }
}
