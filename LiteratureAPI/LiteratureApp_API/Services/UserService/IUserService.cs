using LiteratureApp_API.Entities;
using LiteratureApp_API.Models.Users;

public interface IUserService
{
    AuthenticateResponse Authenticate(AuthenticateRequest model);
    IEnumerable<User> GetAll();
    User GetById(int id);
    void Register(RegisterRequest model);
    void Update(int id, UpdateRequest model);
    void Delete(int id);
    List<(int literatureId, int literatureRating)> GetProfileViewedLiteratures(int id);
}