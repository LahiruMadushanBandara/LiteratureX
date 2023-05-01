using Microsoft.EntityFrameworkCore;
using LiteratureApp_API.Authorization;
using LiteratureApp_API.Helpers;
using LiteratureApp_API.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ML.Data;
using Microsoft.Extensions.ML;
using LiteratureApp_API.Services.LiteratureService;
using LiteratureApp_API.Data_Structures;

var builder = WebApplication.CreateBuilder(args);

// a dd services to DI container
{
    var services = builder.Services;

    // use sql server db in production and sqlite db in development
    services.AddDbContext<DataContext>();

    services.AddCors();
    services.AddControllers();

    // configure automapper with all automapper profiles from this assembly
    services.AddAutoMapper(typeof(Program));

    // configure strongly typed settings object
    services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

    var appSetting = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();

    // configure DI for application services
    services.AddScoped<IJwtUtils, JwtUtils>();
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<ILiteratureService, LiteratureService>();

    services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(x => {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
    services.AddSwaggerGen();

    services.AddPredictionEnginePool<LiteratureRating, LiteratureRatingPrediction>()
        .FromFile(modelName: "LiteratureRecommenderModel", filePath: "Model/model.zip", watchForChanges: true);
}
var app = builder.Build();

// migrate any database changes on startup (includes initial db creation)
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dataContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// configure HTTP request pipeline
{
    // global cors policy
    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

    // global error handler
    app.UseMiddleware<ErrorHandlerMiddleware>();

    app.UseAuthentication();
    app.UseRouting();
    app.UseAuthorization();
    app.MapControllers();
}

app.Run();
