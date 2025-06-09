using api.Models;

namespace api.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByPhoneNumberAsync(string phoneNumber);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(User user);
        Task<bool> UserExistsByEmailAsync(string email);
        Task<bool> UserExistsByPhoneNumberAsync(string phoneNumber);
    }
}