using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

using OgrenciAidatSistemi.Data;
using OgrenciAidatSistemi.Models;

namespace OgrenciAidatSistemi.Services
{
    public class UserService
    {
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetUserById(int userId)
        {
            if (userId == 0)
                return null;
            return await _dbContext.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;
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

        // you may want to edit subclases of User you can use the following method and give your custom static lambda expression as a parameter
        // do update the user's updatedAt timestamp

        public async Task<bool> UpdateUser(User user, Func<User, User> updateExpression)
        {
            try
            {
                user = updateExpression(user);
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

        // abstracted method to use with the SignIn method of subclass controllers
        // you can use this method after checking the user's credentials

        // <summary>
        // Sign in the user and create a cookie for the user (use after checking the user's credentials)
        // </summary>


        public async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.EmailAddress),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            };
            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );
        }

        public async Task SignOutUser()
        {
            if (HttpContext == null)
                return;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public async Task<User?> GetCurrentUser()
        {
            if (HttpContext == null)
                return null;
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return null;
            return await GetUserById(int.Parse(userId));
        }

        public async Task<bool> IsUserSignedIn()
        {
            return HttpContext?.User.Identity?.IsAuthenticated ?? false;
        }



    }
}
