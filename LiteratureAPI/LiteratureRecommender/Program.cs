using LiteratureRecommender;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System.Drawing;
using System.Diagnostics;

string BaseModelRelativePath = @"../../../Model";
string ModelRelativePath = $"{BaseModelRelativePath}/model.zip";

string BaseDataSetRelativepath = @"../../../Data";
string TrainingDataRelativePath = $"{BaseDataSetRelativepath}/ratings_train.csv";
string TestDataRelativePath = $"{BaseDataSetRelativepath}/ratings_test.csv";

string TrainingDataLocation = GetAbsolutePath(TrainingDataRelativePath);
string TestDataLocation = GetAbsolutePath(TestDataRelativePath);
string ModelPath = GetAbsolutePath(ModelRelativePath);


Color color = Color.FromArgb(130, 150, 115);

//Call the following piece of code for splitting the ratings.csv into ratings_train.csv and ratings.test.csv.
// Program.DataPrep();

//STEP 1: Create MLContext to be shared across the model creation workflow objects
MLContext mlContext = new MLContext();

//STEP 2: Read data from text file using TextLoader by defining the schema for reading the literature recommendation datasets and return dataview.
var trainingDataView = mlContext.Data.LoadFromTextFile<LiteratureRating>(path: TrainingDataLocation, hasHeader: true, separatorChar: ',');

Console.WriteLine("=============== Reading Input Files ===============", color);
Console.WriteLine();

// ML.NET doesn't cache data set by default. Therefore, if one reads a data set from a file and accesses it many times, it can be slow due to
// expensive featurization and disk operations. When the considered data can fit into memory, a solution is to cache the data in memory. Caching is especially
// helpful when working with iterative algorithms which needs many data passes. Since SDCA is the case, we cache. Inserting a
// cache step in a pipeline is also possible, please see the construction of pipeline below.
trainingDataView = mlContext.Data.Cache(trainingDataView);

Console.WriteLine("=============== Transform Data And Preview ===============", color);
Console.WriteLine();

//STEP 4: Transform your data by encoding the two features userId and literatureID.
//        These encoded features will be provided as input to FieldAwareFactorizationMachine learner
var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "userIdFeaturized", inputColumnName: nameof(LiteratureRating.userId))
                              .Append(mlContext.Transforms.Text.FeaturizeText(outputColumnName: "literatureIdFeaturized", inputColumnName: nameof(LiteratureRating.literatureId))
                              .Append(mlContext.Transforms.Concatenate("Features", "userIdFeaturized", "literatureIdFeaturized")));
Common.ConsoleHelper.PeekDataViewInConsole(mlContext, trainingDataView, dataProcessPipeline, 10);

// STEP 5: Train the model fitting to the DataSet
Console.WriteLine("=============== Training the model ===============", color);
Console.WriteLine();
var trainingPipeLine = dataProcessPipeline.Append(mlContext.BinaryClassification.Trainers.FieldAwareFactorizationMachine(new string[] { "Features" }));
var model = trainingPipeLine.Fit(trainingDataView);

//STEP 6: Evaluate the model performance
Console.WriteLine("=============== Evaluating the model ===============", color);
Console.WriteLine();
var testDataView = mlContext.Data.LoadFromTextFile<LiteratureRating>(path: TestDataLocation, hasHeader: true, separatorChar: ',');

var prediction = model.Transform(testDataView);

var metrics = mlContext.BinaryClassification.Evaluate(data: prediction, labelColumnName: "Label", scoreColumnName: "Score", predictedLabelColumnName: "PredictedLabel");
Console.WriteLine("Evaluation Metrics: acc:" + Math.Round(metrics.Accuracy, 2) + " AreaUnderRocCurve(AUC):" + Math.Round(metrics.AreaUnderRocCurve, 2), color);

//STEP 7:  Try/test a single prediction by predicting a single literature rating for a specific user
Console.WriteLine("=============== Test a single prediction ===============", color);
Console.WriteLine();
var predictionEngine = mlContext.Model.CreatePredictionEngine<LiteratureRating, LiteratureRatingPrediction>(model);
LiteratureRating testData = new LiteratureRating() { userId = "6", literatureId = "10" };

var literatureRatingPrediction = predictionEngine.Predict(testData);
Console.WriteLine($"UserId:{testData.userId} with literatureId: {testData.literatureId} Score:{Sigmoid(literatureRatingPrediction.Score)} and Label {literatureRatingPrediction.PredictedLabel}", Color.YellowGreen);
Console.WriteLine();

//STEP 8:  Save model to disk
Console.WriteLine("=============== Writing model to the disk ===============", color);
Console.WriteLine(); mlContext.Model.Save(model, trainingDataView.Schema, ModelPath);

Console.WriteLine("=============== Re-Loading model from the disk ===============", color);
Console.WriteLine();
ITransformer trainedModel;
using (FileStream stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
{
    trainedModel = mlContext.Model.Load(stream, out var modelInputSchema);
}

Console.WriteLine("Press any key ...");
Console.Read();


/*
 * FieldAwareFactorizationMachine the learner used in this example requires the problem to setup as a binary classification problem.
 * The DataPrep method performs two tasks:
 * 1. It goes through all the ratings and replaces the ratings > 3 as 1, suggesting a literature is recommended and ratings < 3 as 0, suggesting
      a literature is not recommended
   2. This piece of code also splits the ratings.csv into rating-train.csv and ratings-test.csv used for model training and testing respectively.
 */
void DataPrep()
{

    string[] dataset = File.ReadAllLines(@".\Data\ratings.csv");

    string[] new_dataset = new string[dataset.Length];
    new_dataset[0] = dataset[0];
    for (int i = 1; i < dataset.Length; i++)
    {
        string line = dataset[i];
        string[] lineSplit = line.Split(',');
        double rating = Double.Parse(lineSplit[2]);
        rating = rating > 3 ? 1 : 0;
        lineSplit[2] = rating.ToString();
        string new_line = string.Join(',', lineSplit);
        new_dataset[i] = new_line;
    }
    dataset = new_dataset;
    int numLines = dataset.Length;
    var body = dataset.Skip(1);
    var sorted = body.Select(line => new { SortKey = Int32.Parse(line.Split(',')[3]), Line = line })
                     .OrderBy(x => x.SortKey)
                     .Select(x => x.Line);
    File.WriteAllLines(@"../../../Data\ratings_train.csv", dataset.Take(1).Concat(sorted.Take((int)(numLines * 0.9))));
    File.WriteAllLines(@"../../../Data\ratings_test.csv", dataset.Take(1).Concat(sorted.TakeLast((int)(numLines * 0.1))));
}

float Sigmoid(float x)
{
    return (float)(100 / (1 + Math.Exp(-x)));
}

string GetAbsolutePath(string relativeDatasetPath)
{
    FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
    string assemblyFolderPath = _dataRoot.Directory.FullName;

    string fullPath = Path.Combine(assemblyFolderPath, relativeDatasetPath);

    return fullPath;
}



public class LiteratureRating
{
    [LoadColumn(0)]
    public string userId;

    [LoadColumn(1)]
    public string literatureId;

    [LoadColumn(2)]
    public bool Label;
}

public class LiteratureRatingPrediction
{
    public bool PredictedLabel;

    public float Score;
}