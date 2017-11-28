using Calculon.Models;
using Calculon.Storage;
using System.Linq;
using System.Threading.Tasks;

namespace Calculon.Services
{
    public class UserService
    {
        public async Task<User> AddUserAsync(User user)
        {
            var database = DataContext.Database();

            user.Session = new Session
            {
                State = SessionState.FirstAccess
            };

            user.FirstNumber = 0;
            user.SecondNumber = 0;
            user.Operation = 0;

            database.Users.Add(user);

            return user;
        }

        public async Task<User> GetUserAsync(User user)
        {
            var database = DataContext.Database();

            var userEntity = database.Users.FirstOrDefault(u => u.Node.Name == user.Node.Name);

            if (userEntity == null)
            {
                return await AddUserAsync(user);
            }

            return userEntity;
        }

        public Task UpdateUserSessionAsync(User user)
        {
            var database = DataContext.Database();

            var userEntity = database.Users.FirstOrDefault(u => u.Node.Name == user.Node.Name);
            database.Users.Remove(userEntity);

            if (user.Session != null)
            {
                userEntity.FirstNumber = user.FirstNumber;
                userEntity.SecondNumber = user.SecondNumber;
                userEntity.Operation = user.Operation;
                userEntity.Session = user.Session;
                database.Users.Add(userEntity);
            }

            return Task.CompletedTask;
        }
    }
}