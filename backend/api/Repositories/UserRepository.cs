using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDBContext db;
        public UserRepository(AppDBContext db)
        {
            this.db = db;
        }
        public async Task CreateUserAsync(User user)
        {
            user.CreatedAt = DateTime.Now;
            await db.Users.AddAsync(user);

            await db.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(User user)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await db.Users.Where(u => u.Role == "user")
                .OrderByDescending(u => u.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await db.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByPhoneNumberAsync(string phoneNumber)
        {
            return await db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            db.Users.Update(user);
            await db.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            return await db.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UserExistsByPhoneNumberAsync(string phoneNumber)
        {
            return await db.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }
    }
}