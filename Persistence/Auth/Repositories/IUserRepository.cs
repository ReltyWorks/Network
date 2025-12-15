using Auth.Entities;

namespace Auth.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User GetById(Guid id);
        User GetByUserName(string username);
        void Insert(User user);
        void Update(User user);
        void Delete(Guid id);
        void Save();
    }
}
