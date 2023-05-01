namespace LiteratureApp_API.Services.UserService;

using AutoMapper;
using BCrypt.Net;
using LiteratureApp_API.Models.Users;
using Microsoft.EntityFrameworkCore;
using LiteratureApp_API.Authorization;
using LiteratureApp_API.Entities;
using LiteratureApp_API.Helpers;
using LiteratureApp_API.Models.Users;

public class UserService : IUserService
{
    private DataContext _context;
    private IJwtUtils _jwtUtils;
    private readonly IMapper _mapper;

    public UserService(
        DataContext context,
        IJwtUtils jwtUtils,
        IMapper mapper)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _mapper = mapper;
    }

    private List<UserProfile> _profile = new List<UserProfile>(LoadProfileData());

    public List<UserProfile> GetProfiles => _profile;


    public int _activeprofileid = -1;

    public AuthenticateResponse Authenticate(AuthenticateRequest model)
    {
        var user = _context.Users.SingleOrDefault(x => x.Username == model.Username);

        // validate
        if (user == null || !BCrypt.Verify(model.Password, user.PasswordHash))
            throw new AppException("Username or password is incorrect");

        // authentication successful
        var response = _mapper.Map<AuthenticateResponse>(user);
        response.Token = _jwtUtils.GenerateToken(user);
        return response;
    }

    public IEnumerable<User> GetAll()
    {
        return _context.Users;
    }

    public User GetById(int id)
    {
        return getUser(id);
    }

    public void Register(RegisterRequest model)
    {
        // validate
        if (_context.Users.Any(x => x.Username == model.Username))
            throw new AppException("Username '" + model.Username + "' is already taken");

        // map model to new user object
        var user = _mapper.Map<User>(model);

        // hash password
        user.PasswordHash = BCrypt.HashPassword(model.Password);

        // save user
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public void Update(int id, UpdateRequest model)
    {
        var user = getUser(id);

        // validate
        if (model.Username != user.Username && _context.Users.Any(x => x.Username == model.Username))
            throw new AppException("Username '" + model.Username + "' is already taken");

        // hash password if it was entered
        if (!string.IsNullOrEmpty(model.Password))
            user.PasswordHash = BCrypt.HashPassword(model.Password);

        // copy model to user and save
        _mapper.Map(model, user);
        _context.Users.Update(user);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var user = getUser(id);
        _context.Users.Remove(user);
        _context.SaveChanges();
    }

    // helper methods

    private User getUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        return user;
    }

    

    public List<(int literatureId, int literatureRating)> GetProfileViewedLiteratures(int id)
    {
        foreach (UserProfile Profile in _profile)
        {
            if (id == Profile.ProfileID)
            {
                return Profile.ProfileLiteratureRatings;
            }
        }
        return null;
    }

    public List<UserProfile> GetAllProfiles()
    {
        var userList = _context.Users.ToListAsync().Result;
        var profileList = new List<UserProfile>();
        List<(int literatureId, int literatureRating)> ratings = new List<(int literatureId, int literatureRating)>();



        foreach (var user in userList)
        {
            profileList.Add(new UserProfile()
            {
                ProfileID = user.Id,
                ProfileName = user.Username,
                ProfileImageName = "",
                ProfileLiteratureRatings = ratings
            });
        }
        return _profile;
    }


    private static List<UserProfile> LoadProfileData()
    {
        List<UserProfile> result = new List<UserProfile>();

        FileStream fileReader = File.OpenRead("Data/Profiles.csv");
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
                int ProfileID = int.Parse(fields[0].TrimStart(new char[] { '0' }));
                string ProfileImageName = fields[1];
                string ProfileName = fields[2];

                List<(int literatureId, int literatureRating)> ratings = new List<(int literatureId, int literatureRating)>();

                for (int i = 3; i < fields.Length; i += 2)
                {
                    ratings.Add((int.Parse(fields[i]), int.Parse(fields[i + 1])));
                }

                result.Add(new UserProfile()
                {
                    ProfileID = ProfileID,
                    ProfileImageName = ProfileImageName,
                    ProfileName = ProfileName,
                    ProfileLiteratureRatings = ratings
                });
                index++;
            }
        }
        finally
        {
            if (reader != null)
            {
                reader.Dispose();
            }
        }
        return result;
    }
}