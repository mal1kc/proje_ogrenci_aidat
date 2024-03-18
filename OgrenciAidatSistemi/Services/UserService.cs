using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Services
{
    public class UserService
    {
        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> GetUserById(int userId)
        {
            return await _dbContext.Users.FindAsync(userId);
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
        }

        public async Task<bool> CreateUser(User user)
        {
            try
            {
                // You may want to hash the password before storing it
                user.PasswordHash = User.ComputeHash(user.PasswordHash);

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                return false;
            }
        }

        public async Task<bool> UpdateUser(User user)
        {
            try
            {
                // Update the user's updatedAt timestamp
                user.UpdatedAt = DateTime.UtcNow;

                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                return false;
            }
        }

        public async Task<bool> DeleteUser(int userId)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user != null)
                {
                    _dbContext.Users.Remove(user);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                return false;
            }
        }
    }
}
